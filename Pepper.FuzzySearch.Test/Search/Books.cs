using System.Collections;
using NUnit.Framework;

namespace Pepper.FuzzySearch.Tests.Search
{
    public class Book
    {
        public string Title;
        public string AuthorFirstName;
        public string AuthorLastName;

        public static Book[] Data =
        {
            new() { Title = "Old Mans War", AuthorFirstName = "John", AuthorLastName = "Scalzi" },
            new() { Title = "The Lock Artist", AuthorFirstName = "Steve", AuthorLastName = "Hamilton" },
            new() { Title = "HTML5", AuthorFirstName = "Remy", AuthorLastName = "Sharp" },
            new() { Title = "Right Ho Jeeves", AuthorFirstName = "P.D", AuthorLastName = "Woodhouse" },
            new() { Title = "The Code of the Wooster", AuthorFirstName = "P.D", AuthorLastName = "Woodhouse" },
            new() { Title = "Thank You Jeeves", AuthorFirstName = "P.D", AuthorLastName = "Woodhouse" },
            new() { Title = "The DaVinci Code", AuthorFirstName = "Dan", AuthorLastName = "Brown" },
            new() { Title = "Angels & Demons", AuthorFirstName = "Dan", AuthorLastName = "Brown" },
            new() { Title = "The Silmarillion", AuthorFirstName = "J.R.R", AuthorLastName = "Tolkien" },
            new() { Title = "Syrup", AuthorFirstName = "Max", AuthorLastName = "Barry" },
            new() { Title = "The Lost Symbol", AuthorFirstName = "Dan", AuthorLastName = "Brown" },
            new() { Title = "The Book of Lies", AuthorFirstName = "Brad", AuthorLastName = "Meltzer" },
            new() { Title = "Lamb", AuthorFirstName = "Christopher", AuthorLastName = "Moore" },
            new() { Title = "Fool", AuthorFirstName = "Christopher", AuthorLastName = "Moore" },
            new() { Title = "Incompetence", AuthorFirstName = "Rob", AuthorLastName = "Grant" },
            new() { Title = "Fat", AuthorFirstName = "Rob", AuthorLastName = "Grant" },
            new() { Title = "Colony", AuthorFirstName = "Rob", AuthorLastName = "Grant" },
            new() { Title = "Backwards, Red Dwarf", AuthorFirstName = "Rob", AuthorLastName = "Grant" },
            new() { Title = "The Grand Design", AuthorFirstName = "Stephen", AuthorLastName = "Hawking" },
            new() { Title = "The Book of Samson", AuthorFirstName = "David", AuthorLastName = "Maine" },
            new() { Title = "The Preservationist", AuthorFirstName = "David", AuthorLastName = "Maine" },
            new() { Title = "Fallen", AuthorFirstName = "David", AuthorLastName = "Maine" },
            new() { Title = "Monster 1959", AuthorFirstName = "David", AuthorLastName = "Maine" }
        };

        public static IEnumerable BookTestCases
        {
            get
            {
                yield return new TestCaseData(Data, "hmlt", 1 /* "The Lock Artist" */).SetName("hmlt => The Lock Artist - Steve, Hamilton");
            }
        }
    }
}