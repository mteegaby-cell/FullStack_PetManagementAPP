namespace   PETMSCRUD.Models;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Groomer = "Groomer";
    public const string PetOwner = "PetOwner";
}

public enum Gender { Male, Female }
public enum ServiceStatus { Active, Inactive }
// Frontend should display CheckedIn as "Checked In" and InProgress as "In Progress"
public enum AppointmentStatus { Scheduled, CheckedIn, InProgress, Completed, Cancelled }
