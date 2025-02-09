using System;
using CarDiagnostics.Models;  // ׳”׳•׳¡׳₪׳× ׳”-using ׳׳׳—׳׳§׳× Car


namespace CarDiagnostics.Models
{
 public class Car
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty; // ג… ׳”׳•׳¡׳₪׳× ׳©׳ ׳׳©׳×׳׳©
    public string Email { get; set; } = string.Empty;    // ג… ׳”׳•׳¡׳₪׳× ׳׳™׳׳™׳™׳
    public string Company { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string ProblemDescription { get; set; } = string.Empty;
    public string? AIResponse { get; set; } // ג… ׳”׳•׳¡׳₪׳× ׳©׳“׳” ׳׳×׳©׳•׳‘׳” ׳׳”-AI
}

}
