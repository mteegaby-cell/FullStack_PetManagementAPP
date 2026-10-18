namespace   PETMSCRUD.Models;

public static class Roles
{
    public const string Admin = "ADMIN";
    public const string Groomer = "GROOMER";
    public const string PetOwner = "PETOWNER";
}

public enum Gender { Male, Female }
public enum ServiceStatus { Active, Inactive }
// Frontend should display CheckedIn as "Checked In" and InProgress as "In Progress"
public enum AppointmentStatus { Scheduled, CheckedIn, InProgress, Completed, Cancelled }
