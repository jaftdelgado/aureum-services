using System.ComponentModel.DataAnnotations;

namespace PortfolioService.Dtos
{
    /// <summary>
    /// DTO para la gestión y actualización de la billetera (Wallet) de un usuario.
    /// </summary>
    public class WalletDto
    {
        /// <summary>
        /// Identificador de la membresía asociada a la wallet.
        /// </summary>
        [Required(ErrorMessage = "El MembershipId es obligatorio.")]
        public Guid MembershipId { get; set; }

        /// <summary>
        /// Nuevo saldo de efectivo total a establecer en la wallet.
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "El saldo no puede ser negativo.")]
        public decimal CashBalance { get; set; }
    }
    /// <summary>
    /// Respuesta tras actualizar el saldo de una billetera.
    /// </summary>
    public class WalletUpdateResponseDto
    {
        /// <summary>
        /// Mensaje del resultado.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// ID de la billetera afectada.
        /// </summary>
        public Guid WalletId { get; set; }

        /// <summary>
        /// Nuevo saldo disponible en la billetera.
        /// </summary>
        /// <example>5000.00</example>
        public decimal Balance { get; set; }
    }
}