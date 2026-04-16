using System;
using System.Collections.Generic;
using System.Text;

namespace Tienda.Models;

public class Address
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = "México";
    public bool IsDefault { get; set; }
}