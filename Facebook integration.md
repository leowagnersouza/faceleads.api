# Integração de Leads do Facebook (Lead Ads)  
`facebook-leads-integration.md`

## Visão geral

Esta documentação descreve a integração para receber **leads gerados em formulários de anúncios do Facebook (Lead Ads)**, 
com foco em:

- Estrutura do **Webhook de Lead** (evento `leadgen`)
- Estrutura da **Lead Retrieval API** (consulta dos dados do lead)
- Modelo de **banco de dados recomendado**
- Exemplos de payloads em JSON
- Notas e considerações importantes

O objetivo é servir como documento de referência técnica para o projeto da API em .NET.

---

## Fluxo geral da integração

1. **Usuário preenche um formulário de Lead Ads** em um anúncio no Facebook.
2. O Facebook dispara um **Webhook** para a URL configurada, com um evento do tipo `leadgen`.
3. O payload do Webhook contém **metadados do lead**, incluindo o `leadgen_id`.
4. A API da sua aplicação recebe esse Webhook e:
   - Valida a assinatura/autenticidade (se configurado).
   - Extrai o `leadgen_id`.
5. A aplicação chama a **Lead Retrieval API** do Facebook:
   - `GET https://graph.facebook.com/vX.X/{leadgen_id}?access_token=...`
6. A resposta contém os **dados do formulário preenchido** (`field_data`).
7. A aplicação persiste os dados no banco, usando o modelo descrito abaixo.

---

## Estrutura do Webhook de Lead (Leadgen)

Quando um lead é gerado, o Facebook envia um POST para o endpoint configurado como Webhook.  
O corpo da requisição tem, em linhas gerais, a seguinte estrutura:

```json
{
  "object": "page",
  "entry": [
    {
      "id": "PAGE_ID",
      "time": 1458692752478,
      "changes": [
        {
          "field": "leadgen",
          "value": {
            "ad_id": "1234567890",
            "form_id": "987654321",
            "leadgen_id": "1122334455667788",
            "created_time": "2024-01-01T12:34:56+0000",
            "page_id": "PAGE_ID",
            "adgroup_id": "22334455",
            "campaign_id": "99887766"
          }
        }
      ]
    }
  ]
}

Campos Importantes
leadgen_id → ID único do lead (usado para buscar os dados completos)
form_id → ID do formulário
ad_id, adgroup_id, campaign_id → metadados de campanha
created_time → data/hora do lead

O Webhook não contém os dados do formulário (nome, e-mail, telefone etc.).

Estrutura da Lead Retrieval API
Para obter os dados completos do lead, é necessário consultar:
GET https://graph.facebook.com/v19.0/{leadgen_id}?access_token=...

Exemplo de Resposta
json
{
  "created_time": "2024-01-01T12:34:56+0000",
  "id": "1122334455667788",
  "field_data": [
    {
      "name": "full_name",
      "values": ["João da Silva"]
    },
    {
      "name": "email",
      "values": ["joao@email.com"]
    },
    {
      "name": "phone_number",
      "values": ["+5511999999999"]
    },
    {
      "name": "custom_question_1",
      "values": ["Resposta da pergunta"]
    }
  ]
}

Observações
field_data é uma lista de campos dinâmicos.

Cada campo contém:
name
values (sempre array)

Modelo de Banco de Dados Recomendado
Como os campos podem variar entre formulários, recomenda-se um modelo flexível.

Tabela: leads
Armazena informações gerais e metadados.

Campo	Tipo sugerido	Descrição
id	string / GUID	leadgen_id do Facebook (PK)
form_id	string	ID do formulário
ad_id	string	ID do anúncio
adgroup_id	string	ID do conjunto de anúncios
campaign_id	string	ID da campanha
page_id	string	ID da página
created_time	datetime	Data/hora do lead
received_at	datetime	Quando o Webhook foi recebido
raw_payload	json / text	Payload bruto (opcional)

Tabela: lead_fields
Armazena os campos do formulário em formato chave–valor.

Campo	Tipo sugerido	Descrição
id	auto / int	PK
lead_id	string	FK para leads.id
field_name	string	Nome do campo
field_value	string	Valor do campo


Exemplo Completo de Fluxo

1. Receber o Webhook
Extrair:
leadgen_id
form_id
ad_id
adgroup_id
campaign_id
page_id
created_time

Salvar em leads.

2. Consultar a Lead Retrieval API
Usar:
Código
GET /{leadgen_id}

3. Persistir os Campos
Para cada item em field_data:

Inserir em lead_fields:
lead_id
field_name
field_value

Considerações Importantes
Campos Dinâmicos
Formulários podem mudar ao longo do tempo.

Campos personalizados são comuns.
O modelo chave–valor evita alterações no schema.

Idempotência
O mesmo leadgen_id nunca deve ser inserido duas vezes.

Trate duplicatas como atualização ou ignore.

Segurança
Use HTTPS no Webhook.
Valide tokens/assinaturas se configurado.
Não exponha o access_token.

Auditoria
Armazenar o raw_payload ajuda em debug e reprocessamento.
