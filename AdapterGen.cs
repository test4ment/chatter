using Scriban;

public static class AdapterGenerator<Target> where Target : notnull
{
    private static Dictionary<Type, Dictionary<Type, string>> known_code = new(); 
    public static string GenerateCodeString(Type source)
    {
        var type = typeof(Target);

        try
        {
            return known_code[source][type]; // return IoC.Get<string>("Adapters.Code.Get", source, type);
        }
        catch (KeyNotFoundException)
        {
            // var lexemes_dictionary = IoC.Get<IDictionary<string, string>>("Adapters.Source.Lexemes", source);

            var template = Template.Parse(@"public class {{interface_name}}Adapter : {{interface_name}}
            {
                private readonly {{source_type}} _obj;
                public {{interface_name}}Adapter({{source_type}} obj)
                {
                    _obj = obj;
                }
                {{~ for property in properties ~}}
                    public {{property.type}} {{property.name}} {
                        {{if property.can_read }} get => ({{property.type}})_obj.properties.Get(""{{property.name | string.capitalize | string.replace ""_"" "" ""}}""); {{~ end}}
                        {{if property.can_write }} set => _obj.properties.Set(""{{property.name | string.capitalize | string.replace ""_"" "" ""}}"", value); {{~ end}}
                    }
                {{~ end ~}}
            }");

            var render = template.Render(
                new
                {
                    interface_name = type.Name,
                    // lexemes = lexemes_dictionary,
                    source_type = source.Name,
                    properties = type.GetProperties().Select(a => new
                    {
                        can_read = a.CanRead,
                        can_write = a.CanWrite,
                        type = a.PropertyType.ToString(),
                        name = a.Name
                    }).ToList()
                }
            );

            try{
                known_code[source][type] = render;
            }
            catch(KeyNotFoundException){
                known_code[source] = new Dictionary<Type, string>(){
                    {type, render}
                };
            }
            // IoC.Get<ICommand>("Adapters.Code.Add", source, type, render).Execute();

            return render;
        }
    }
}
