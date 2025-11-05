using Amazon.EC2; // Biblioteca do SDK AWS para manipular recursos EC2 (Elastic Compute Cloud)
using Amazon.EC2.Model; // Modelos de requisição e resposta da API EC2
using Amazon.Runtime; // Contém classes de autenticação (credenciais AWS)
using IniParser; // Biblioteca externa para leitura e escrita de arquivos INI
using IniParser.Model; // Modelos usados pelo IniParser 
using System;
using System.Collections.Generic; 
using System.Net; // Usado para obter o IP público via WebClient 

namespace SW_AWS_IP_UPDATE
{ 
    class Program
    {
        // Caminho base do executável (pasta onde o .exe está sendo executado)
        public static string pasta = AppDomain.CurrentDomain.BaseDirectory;

        // Nome/descrição do cliente (lido do arquivo INI)
        public static string cliente = "Local";

        // Método simples de log (aqui apenas escreve no console)
        public static void SalvaLog(string titulo, string msg)
        {
            try
            {
                Console.WriteLine(msg);
            }
            catch { } // Ignora erros no console (sem travar a aplicação)
        }

        static void Main(string[] args)
        {
            try
            {  
                string erro = ""; // string acumuladora de mensagens de erro
                string ip = ""; // IP atual (obtido pela internet)
                string ip_antigo = ""; // IP armazenado anteriormente no INI

                // Obtém o IP público atual do servidor/máquina via site ipinfo.io
                ip = new WebClient().DownloadString("https://ipinfo.io/ip");

                // Carrega o arquivo de configuração INI
                var parser = new FileIniDataParser();
                IniData data = parser.ReadFile(pasta + "swconfigIP.ini");

                // Mostra IP atual e o IP gravado no arquivo
                SalvaLog("IP local", ip.Trim());
                SalvaLog("IP Ini", data["DATA"]["IP"].Trim());

                // Lê a descrição do cliente do arquivo INI
                cliente = data["DATA"]["DESCRICAO"].Trim();

                // Compara se o IP atual é diferente do IP armazenado
                if (ip.Trim() != data["DATA"]["IP"].Trim())
                {
                    try
                    {
                        // Guarda o IP antigo
                        ip_antigo = data["DATA"]["IP"];

                        // Atualiza o arquivo INI com o novo IP
                        data["DATA"]["IP"] = ip.Trim();
                        parser.WriteFile(pasta + "swconfigIP.ini", data);
                    }
                    catch (Exception e)
                    {
                        // Caso falhe ao gravar o INI, registra erro
                        SalvaLog("IP local", e.Message);
                        erro = " Salvando Ini -> " + e.Message;
                    }

                    SalvaLog("IP local", "Salvou Ini, vai conectar na amazon");

                    BasicAWSCredentials awsCredentials = null;

                    // Cria credenciais AWS (chave e segredo fixos — **NUNCA deve estar hardcoded**)
                    awsCredentials = new Amazon.Runtime.BasicAWSCredentials("1111", "1111");

                    try
                    {
                        // Cria cliente EC2 apontando para a região São Paulo (sa-east-1)
                        AmazonEC2Client client = new AmazonEC2Client(awsCredentials, Amazon.RegionEndpoint.SAEast1);

                        SalvaLog("IP local", "Vai dar revoke no antigo");

                        try
                        {
                            // Revoga (remove) a regra anterior do Security Group
                            var response_revoke = client.RevokeSecurityGroupIngress(new RevokeSecurityGroupIngressRequest
                            {
                                GroupId = "123", // ID do Security Group no EC2
                                IpPermissions = new List<IpPermission> {
                                    new IpPermission {
                                        FromPort = 1433, // Porta padrão do SQL Server
                                        IpProtocol = "tcp",
                                        ToPort = 1433,
                                        Ipv4Ranges = new List<IpRange> {
                                            new IpRange {
                                                CidrIp = ip_antigo+"/32" // Remove acesso do IP antigo
                                            }
                                        }
                                    }
                                }
                            });
                        }
                        catch (Exception e)
                        {
                            // Caso falhe ao remover a regra
                            SalvaLog("IP local", e.Message);
                            erro = erro + " Amazon -> " + e.Message;
                        }

                        SalvaLog("IP local", "Vai dar authorize no atual");

                        try
                        {
                            // Autoriza o novo IP no Security Group
                            var response = client.AuthorizeSecurityGroupIngress(new AuthorizeSecurityGroupIngressRequest
                            {
                                GroupId = "123",
                                IpPermissions = new List<IpPermission> {
                                    new IpPermission {
                                        FromPort = 1433,
                                        IpProtocol = "tcp",
                                        ToPort = 1433,
                                        Ipv4Ranges = new List<IpRange> {
                                            new IpRange {
                                                CidrIp = ip.Trim() + "/32", // Libera apenas o IP atual
                                                Description = cliente // Coloca o nome do cliente como descrição
                                            }
                                        }
                                    }
                                }
                            });
                        }
                        catch (Exception e)
                        {
                            // Caso falhe ao adicionar nova regra
                            SalvaLog("IP local", e.Message);
                            erro = erro + " Amazon -> " + e.Message;
                        }
                    }
                    catch (Exception e)
                    {
                        // Erro genérico na conexão ou autenticação AWS
                        SalvaLog("IP local", e.Message);
                        erro = erro + " Amazon -> " + e.Message;
                    }
                }
            }
            catch (Exception e)
            {
                // Captura falhas gerais (como falha ao buscar IP ou ler o arquivo)
                SalvaLog("IP local", e.Message);
            }
        }
    }
}
