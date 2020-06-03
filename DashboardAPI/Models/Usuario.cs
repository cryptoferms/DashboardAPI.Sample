using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DashboardAPI.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string Sobrenome { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Senha { get; set; }
        [NotMapped]
        public string ConfirmarSenha { get; set; }
        public string Telefone { get; set; }
        public string Endereco { get; set; }
        public string ImageUrl { get; set; }
        public string DataNascimento { get; set; }
        public DateTime DataInclusao { get; set; }
        public char Status { get; set; }
    }
}
