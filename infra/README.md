# Infra Bicep - Faceleads

Este diretório contém templates Bicep e exemplos para provisionamento de recursos Azure usados pelo projeto Faceleads.

Arquivos principais
- `create-rg.bicep` - cria um Resource Group (escopo de subscription). Usado uma vez por subscription/ambiente.
- `sql-server-db.bicep` - cria SQL Server + Database e regras de firewall (escopo de resource group).
- `params.dev.json` - exemplo de parâmetros para ambiente `dev` (não contém segredos reais).
- `params.prod.json` - exemplo de parâmetros para ambiente `prod` (não contém segredos reais).

Princípios
- Use Bicep para infraestrutura declarativa. Mantemos versões no Git para reprodutibilidade e auditoria.
- Não commit secrets. Use parâmetros ou variáveis de CI (GitHub Actions Secrets) quando automatizar.

Como usar (local)
1. Autentique-se no Azure:

   ```bash
   az login
   az account set --subscription "<SUBSCRIPTION_ID_OR_NAME>"
   ```

2. Criar Resource Group (uma vez):

   ```bash
   az deployment sub create \
     --location brazilsouth \
     --template-file infra/create-rg.bicep \
     --parameters rgName=rg-faceleads-dev location=brazilsouth
   ```

3. Deploy do SQL Server + Database (dentro do RG):

   - Usando parâmetros inline (não recomendado em CI):

   ```bash
   az deployment group create -g rg-faceleads-dev \
     --template-file infra/sql-server-db.bicep \
     --parameters sqlServerName=faceleads-sql-dev administratorLogin=sqladmin administratorLoginPassword='YourP@ssw0rd' databaseName=Faceleads firewallStartIp='<YOUR_IP>'
   ```

   - Usando arquivo de parâmetros (ex.: `infra/params.dev.json`):

   ```bash
   az deployment group create -g rg-faceleads-dev \
     --template-file infra/sql-server-db.bicep \
     --parameters @infra/params.dev.json
   ```

4. Verificar outputs do deployment:

   ```bash
   az deployment group show -g rg-faceleads-dev --name <deploymentName> --query properties.outputs -o json
   ```
5. Criar app service plan

- SQL Server: `faceleads-sql-dev` (FQDN: `faceleads-sql-dev.database.windows.net`)
- Firewall rule: `Allow_179_110_139_234` (179.110.139.234)
- App Service Plan: `faceleads-plan-dev` (SKU: `F1`, Linux)
  - Comando executado:
    ```
    az appservice plan create --name faceleads-plan-dev --resource-group rg-faceleads-dev --sku F1 --is-linux --output json
    ```

Boas práticas / notas
- Para CI/CD (GitHub Actions) use `azure/login` e chame `az deployment group create` com parâmetros passados por secrets (`administratorLoginPassword`, `firewallStartIp`, etc.).
- Evite passar senhas na linha de comando em máquinas públicas; prefira variáveis de ambiente protegidas no runner.
- Para desenvolvimento local é aceitável usar o valor padrão temporário, mas remova antes de produção.
- Depois de provisionar, rode `deploy/migrate-db.sh` apontando para a connection string de produção/dev conforme necessário.

Próximos passos sugeridos
- Gerar Bicep para App Service e Static Web App.
- Criar GitHub Action que executa Bicep deploy para `dev` (automatic) e `prod` (workflow_dispatch manual).

Se quiser, eu gero agora o workflow de deploy para `dev` (push -> deploy) ou gero o Bicep para App Service/Static Web App. Diga qual prefere.

6. Criar Web App (executado)

- Comando executado:

```bash
az webapp create \
  --resource-group rg-faceleads-dev \
  --plan faceleads-plan-dev \
  --name faceleads-api-dev \
  --runtime "DOTNETCORE:9.0" \
  --output json
```

- Resultado resumido (exemplo retornado pelo comando):

  - `defaultHostName`: `faceleads-api-dev.azurewebsites.net`
  - `state`: `Running`
  - `resourceGroup`: `rg-faceleads-dev`
  - `serverFarmId`: `/subscriptions/<sub-id>/resourceGroups/rg-faceleads-dev/providers/Microsoft.Web/serverfarms/faceleads-plan-dev`

Observação: não avancei para o próximo passo (configurar connection string / deploy) — aguardo sua confirmação para prosseguir.

7. Configurar Connection String (executado)

- Comando executado:

```bash
az webapp config connection-string set \
  --resource-group rg-faceleads-dev \
  --name faceleads-api-dev \
  --settings LeadsDatabase="Server=tcp:faceleads-sql-dev.database.windows.net,1433;Initial Catalog=Faceleads;Persist Security Info=False;User ID=sqladmin;Password=Gi@ele0804;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" \
  --connection-string-type SQLAzure \
  --output json
```

- Resultado resumido (exemplo retornado pelo comando):

```json
{
  "LeadsDatabase": {
    "type": "SQLAzure",
    "value": null
  }
}
```

Observação importante: o SDK/CLI normalmente retorna `value: null` por motivos de segurança — isso não significa que a connection string não foi aplicada. O App Service armazena a string de conexão e a expõe à aplicação em runtime (sobrescrevendo `appsettings`), mas não retorna o valor em texto claro. Para verificar que está definida, liste as connection strings ou acesse o Portal:

```bash
az webapp config connection-string list --resource-group rg-faceleads-dev --name faceleads-api-dev --output json
```

8. Criar Service Principal para CI/CD (executado)

- Comando executado (uma linha):

```bash
az ad sp create-for-rbac --name "faceleads-deploy-sp" --role contributor --scopes /subscriptions/d0a091d8-e4a4-4e64-ab04-be69928a9d30/resourceGroups/rg-faceleads-dev -o json
```

- Instruções pós-criação:

  1. O comando retorna um JSON contendo `appId`, `password` e `tenant`. Copie toda a saída JSON.
  2. No GitHub do repositório, adicione um Secret (Settings ? Secrets ? Actions) chamado `AZURE_CREDENTIALS` com esse JSON como valor. Esse JSON será usado pelo `azure/login` no workflow.
  3. Adicione também o secret `AZURE_WEBAPP_NAME` com o valor `faceleads-api-dev`.

- Observações de segurança:

  - Não commite o JSON do service principal no repositório.
  - Restrinja o `--scopes` ao Resource Group em vez de toda a subscription quando possível.
  - Rotacione a credencial (`password`) periodicamente e remova o SP quando não for mais necessário.
