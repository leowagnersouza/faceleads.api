using System.Collections.Generic;

namespace Faceleads.Leads.Domain;

public sealed class Usuario
{
    public Guid Id { get; private set; }

    // Identificador do tenant (não-nulo)
    public Guid TenantId { get; private set; }

    // Se o usuário estiver vinculado a um consultor, armazena o id
    public Guid? ConsultorId { get; private set; }

    // Dados de login
    public string NomeUsuario { get; private set; } = string.Empty;
    public string NormalizedNomeUsuario { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;
    public string NormalizedEmail { get; private set; } = string.Empty;

    // Senha (hash)
    public string SenhaHash { get; private set; } = string.Empty;
    public string? SecurityStamp { get; private set; }

    // Estado da conta
    public bool EmailConfirmado { get; private set; }
    public bool Ativo { get; private set; }

    // Lockout / tentativas
    public int TentativasFalhaAcesso { get; private set; }
    public bool BloqueioHabilitado { get; private set; }
    public DateTimeOffset? BloqueioFim { get; private set; }

    // Auditoria
    public DateTime? CreatedOn { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? ModifiedOn { get; private set; }
    public string? ModifiedBy { get; private set; }

    // Navegações (inicializadas para evitar null refs)
    public IReadOnlyCollection<UsuarioRole> Roles { get; private set; } = new List<UsuarioRole>();
    public IReadOnlyCollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    private Usuario()
    {
        // Requerido pelo EF Core
    }

    public Usuario(Guid tenantId, string nomeUsuario, string email, string senhaHash)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        NomeUsuario = nomeUsuario;
        NormalizedNomeUsuario = nomeUsuario.ToUpperInvariant();
        Email = email;
        NormalizedEmail = email.ToUpperInvariant();
        SenhaHash = senhaHash;
        SecurityStamp = Guid.NewGuid().ToString();
        EmailConfirmado = false;
        Ativo = true;
        TentativasFalhaAcesso = 0;
    }

    public void AtualizarContato(string nomeUsuario, string email)
    {
        NomeUsuario = nomeUsuario;
        NormalizedNomeUsuario = nomeUsuario.ToUpperInvariant();
        Email = email;
        NormalizedEmail = email.ToUpperInvariant();
    }

    public void SetSenhaHash(string senhaHash)
    {
        SenhaHash = senhaHash;
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public void RegisterFailedAccess()
    {
        TentativasFalhaAcesso++;
    }

    public void ResetFailedAccess()
    {
        TentativasFalhaAcesso = 0;
    }

    public void EnableLockout(bool enabled)
    {
        BloqueioHabilitado = enabled;
    }

    public void SetLockout(DateTimeOffset until)
    {
        BloqueioFim = until;
    }

    public void Ativar()
    {
        Ativo = true;
    }

    public void Desativar()
    {
        Ativo = false;
    }
}
