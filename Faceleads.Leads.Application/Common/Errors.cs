namespace Faceleads.Leads.Application.Common;

public static class Errors
{
    public static readonly Error Generic = new("ERROR", "An error occurred", 400);
    public static readonly Error AuthInvalid = new("AUTH_INVALID", "Credenciais inválidas.", 401);
    public static readonly Error RefreshInvalid = new("REFRESH_INVALID", "Refresh token is invalid or expired", 401);

    public static readonly Error ConsultorNomeObrigatorio = new("CONSULTOR_NOME_OBRIGATORIO", "Nome completo do consultor é obrigatório.", 400);
    public static readonly Error ConsultorEmailObrigatorio = new("CONSULTOR_EMAIL_OBRIGATORIO", "Email do consultor é obrigatório.", 400);
    public static readonly Error ConsultorIdInvalido = new("CONSULTOR_ID_INVALIDO", "O identificador do consultor é inválido.", 400);
    public static readonly Error ConsultorNaoEncontrado = new("CONSULTOR_NAO_ENCONTRADO", "Consultor não encontrado.", 404);

    // Usuario errors
    public static readonly Error UsuarioNomeObrigatorio = new("USUARIO_NOME_OBRIGATORIO", "Nome de usuário é obrigatório.", 400);
    public static readonly Error UsuarioEmailObrigatorio = new("USUARIO_EMAIL_OBRIGATORIO", "Email do usuário é obrigatório.", 400);
    public static readonly Error UsuarioJaExiste = new("USUARIO_JA_EXISTE", "Um usuário com este nome de usuário ou email já existe.", 400);
    public static readonly Error UsuarioSenhaObrigatoria = new("USUARIO_SENHA_OBRIGATORIA", "Senha é obrigatória.", 400);
    public static readonly Error UsuarioSenhaCurta = new("USUARIO_SENHA_CURTA", "Senha deve conter pelo menos 6 caracteres.", 400);
}
