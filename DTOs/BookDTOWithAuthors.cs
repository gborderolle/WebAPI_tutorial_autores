namespace WebAPI_tutorial_recursos.DTOs
{
    /// <summary>
    /// Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/26946928#notes
    /// </summary>
    public class BookDTOWithAuthors : BookDTO
    {
        public List<AuthorDTO> AuthorList { get; set; } // 0..n (0=no existe Review sin Book)
    }
}
