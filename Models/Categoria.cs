using System.Text.Json.Serialization;

namespace MinimalApiCatalogo.Models
{
    public class Categoria
    {
        public int CategoriaId { get; set; }
        public string? Nome { get; set; }
        public string? Descricao { get; set; }

        //Propriedade de navegação. Usado apenas para fazer o mapeamento
        [JsonIgnore]
        public ICollection<Produto>? Produtos { get; set; }
    }
}
