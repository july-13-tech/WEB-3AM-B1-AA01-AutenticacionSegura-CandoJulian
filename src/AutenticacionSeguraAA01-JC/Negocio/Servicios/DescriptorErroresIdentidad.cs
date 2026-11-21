using Microsoft.AspNetCore.Identity;

namespace AutenticacionSeguraAA01_JC.Negocio.Servicios;

public class DescriptorErroresIdentidad : IdentityErrorDescriber
{
    public override IdentityError DefaultError()
    {
        return new IdentityError { Code = nameof(DefaultError), Description = "Ocurrió un error inesperado." };
    }

    public override IdentityError PasswordTooShort(int length)
    {
        return new IdentityError { Code = nameof(PasswordTooShort), Description = $"La contraseña debe tener al menos {length} caracteres." };
    }

    public override IdentityError PasswordRequiresDigit()
    {
        return new IdentityError { Code = nameof(PasswordRequiresDigit), Description = "La contraseña debe incluir al menos un número." };
    }

    public override IdentityError PasswordRequiresLower()
    {
        return new IdentityError { Code = nameof(PasswordRequiresLower), Description = "La contraseña debe incluir al menos una letra minúscula." };
    }

    public override IdentityError PasswordRequiresUpper()
    {
        return new IdentityError { Code = nameof(PasswordRequiresUpper), Description = "La contraseña debe incluir al menos una letra mayúscula." };
    }

    public override IdentityError PasswordRequiresNonAlphanumeric()
    {
        return new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "La contraseña debe incluir al menos un símbolo." };
    }

    public override IdentityError PasswordMismatch()
    {
        return new IdentityError { Code = nameof(PasswordMismatch), Description = "La contraseña actual es incorrecta." };
    }

    public override IdentityError InvalidToken()
    {
        return new IdentityError { Code = nameof(InvalidToken), Description = "El enlace no es válido o ha vencido." };
    }

    public override IdentityError DuplicateEmail(string email)
    {
        return new IdentityError { Code = nameof(DuplicateEmail), Description = "El correo ya está registrado." };
    }

    public override IdentityError DuplicateUserName(string userName)
    {
        return new IdentityError { Code = nameof(DuplicateUserName), Description = "El correo ya está registrado." };
    }

    public override IdentityError InvalidEmail(string? email)
    {
        return new IdentityError { Code = nameof(InvalidEmail), Description = "El correo no tiene un formato válido." };
    }
}