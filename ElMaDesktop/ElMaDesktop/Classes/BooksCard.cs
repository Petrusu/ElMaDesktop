using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using Avalonia.Media.Imaging;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace ElMaDesktop.Classes;

public class BooksCard
{
    public int BookId { get; set; }

    public string Title { get; set; } = null!;

    public string? SeriesName { get; set; }

    public string? Annotation { get; set; }

    public string Publisher { get; set; }

    public string? PlaceOfPublication { get; set; }

    public string YearOfPublication { get; set; }
    public string Bbkcode { get; set; }
    public string? Author { get; set; }
    public string? Editor { get; set; }
    public ObservableCollection<string> Themes { get; set; } = new ObservableCollection<string>();

    public byte[]? Image { get; set; }
    public Avalonia.Media.Imaging.Bitmap? ImageBook
    {
        get
        {
            if (Image == null || Image.Length == 0)
                return null;

            using (var stream = new MemoryStream(Image))
            {
                return new Avalonia.Media.Imaging.Bitmap(stream);
            }
        }
    }
    public List<string> Authors { get; set; } = new List<string>();
    public List<string> Editors { get; set; } = new List<string>();
}

public class Author
{
    public string Authorsname { get; set; } = null!;
}

public class Editor
{
    public string Editorname { get; set; } = null!;
}
