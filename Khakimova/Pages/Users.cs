public partial class User
{
    public int ID { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public string FIO { get; set; }
    public string Role { get; set; }
    public byte[] Photo { get; set; } // Должно быть byte[], а не string
}