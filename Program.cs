using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using GraphQL.SystemTextJson;



public class SampleData
{
    public static IEnumerable<Droid> getDroids()
    {
        return new List<Droid>() {
            new Droid(){ Id = "1", Name = "R2-D2" },
            new Droid(){ Id = "2", Name = "C3-PO" },
            new Droid(){ Id = "3", Name = "R2-D3" },
        };
    }
}
// Definition of Droid's fields
public class Droid
{
  public string Id { get; set; }
  public string Name { get; set; }
}

// Definition of Character's fields
public class Character
{
  public string Name { get; set; }
}

[GraphQLMetadata("Droid", IsTypeOf=typeof(Droid))]
public class DroidType
{
  public string Id([FromSource] Droid droid) => droid.Id;
  public string Name([FromSource] Droid droid) => droid.Name;

  // these two parameters are optional
  // IResolveFieldContext provides contextual information about the field
  public Character Friend(IResolveFieldContext context, [FromSource] Droid source)
  {
    return new Character { Name = "C3-PO" };
  }
}

public class Query 
{
    [GraphQLMetadata("droid")]
    public Droid GetDroid(String id)
    {
        return SampleData.getDroids().SingleOrDefault(j => j.Id == id);
    }
    [GraphQLMetadata("droids")]
    public IEnumerable<Droid> GetDroids()
    {
        return SampleData.getDroids();
    }
}

public class Program
{
    public static async Task Main(string[] args)
    {
        var schema = Schema.For(@"
          type Droid {
            id: String!
            name: String!
            friend: Character
          }

          type Character {
            name: String!
          }

          type Query {
            droid(id: ID): Droid
            droids: [Droid]
          }
        ", _ =>
        {
            _.Types.Include<DroidType>();
            _.Types.Include<Query>();
        });

        var json = await schema.ExecuteAsync(_ =>
        {
            _.Query = "{ droid(id: \"1\") { id, name, friend { name } } }";
        });

        Console.WriteLine(json);
    }
}