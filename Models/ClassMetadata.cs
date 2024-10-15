using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Edusync.Data;

public class ClassMetadata
{
    [Display(Name="Teacher")]
    public int TeachersId { get; set; }

    [Display(Name="Course")]
    public int CourseId { get; set; }
}

[ModelMetadataType(typeof(ClassMetadata))]
public partial class Class{}

