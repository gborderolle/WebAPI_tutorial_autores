namespace WebAPI_tutorial_recursos.Utilities.HATEOAS
{
    // Clase: https://www.udemy.com/course/construyendo-web-apis-restful-con-aspnet-core/learn/lecture/27148814#notes
    public class ResourcesCollection<T> : Resource where T : Resource
    {
        public List<T> Values { get; set; }
    }
}
