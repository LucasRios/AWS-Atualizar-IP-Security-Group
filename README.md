SW_AWS_IP_UPDATE

Atualize automaticamente seu IP público nas regras de acesso da AWS! 🌐

Este projeto em C# verifica o IP público atual da máquina e, caso tenha mudado, atualiza automaticamente o Security Group da AWS — revogando o IP antigo e autorizando o novo. Ideal para quem usa conexões de SQL Server hospedadas na AWS com IP dinâmico! ⚙️

🛠️ Funcionalidades

✅ Obtém automaticamente o IP público via ipinfo.io

🔁 Compara o IP atual com o registrado no arquivo swconfigIP.ini
📝 Atualiza o arquivo INI com o novo IP
☁️ Conecta à AWS EC2 via SDK e atualiza o Security Group
🚫 Remove o IP antigo e 🔓 adiciona o novo IP na porta 1433 (SQL Server)
🧩 Logs diretos no console informando cada etapa e erros

📦 Requisitos

.NET Framework 4.7+ ou .NET 6+

SDK da AWS instalado (AWSSDK.EC2, AWSSDK.Core)

Biblioteca IniParser

Credenciais AWS válidas (Access Key e Secret Key)

Permissão para modificar regras de Security Groups na conta AWS

⚙️ Estrutura do Arquivo INI (swconfigIP.ini)
[DATA]
IP=0.0.0.0
DESCRICAO=ClienteExemplo


IP: último IP registrado

DESCRICAO: nome ou identificação do cliente

🚀 Como Funciona

O programa lê o IP público atual (https://ipinfo.io/ip).

Compara com o valor salvo em swconfigIP.ini.

Se o IP mudou:

Atualiza o arquivo INI.

Conecta na AWS EC2.

Revoga a regra antiga do Security Group (porta 1433).

Autoriza o novo IP.

Exibe logs no console com todas as ações executadas.

🧩 Exemplo de Log no Console
IP local: 177.54.23.101
IP Ini: 177.54.20.88
Salvou Ini, vai conectar na amazon
Vai dar revoke no antigo
Vai dar authorize no atual

⚠️ Observações Importantes

🔐 NUNCA armazene as credenciais da AWS diretamente no código.
💾 Prefira usar variáveis de ambiente ou o arquivo de credenciais da AWS (~/.aws/credentials).
🧱 Execute em ambiente controlado, pois o código altera regras de firewall em tempo real.
🌍 Certifique-se de usar a região correta (Amazon.RegionEndpoint.SAEast1 para São Paulo).

📥 Compilação

Abra o projeto no Visual Studio.

Restaure os pacotes NuGet:

Install-Package AWSSDK.EC2
Install-Package AWSSDK.Core
Install-Package IniParser


Compile em modo Release.

Copie o executável e o arquivo swconfigIP.ini para a mesma pasta.

💡 Uso Típico

Execute o programa manualmente ou via agendador de tarefas do Windows para atualizar o IP automaticamente sempre que ele mudar.

📜 Licença

Distribuído gratuitamente para uso profissional e educacional.
Sinta-se livre para contribuir, adaptar ou melhorar conforme sua necessidade. 🤝

Feito com ⚙️ C# e AWS SDK.
