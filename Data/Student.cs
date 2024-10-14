using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Edusync.Data;

public partial class Student
{
    public int Id { get; set; }
    
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }
}
