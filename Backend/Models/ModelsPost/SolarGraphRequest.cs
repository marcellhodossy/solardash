namespace SolarDash.Models {
    public class SolarGraphRequest {
        public int Id {get;set;}
        public float Longitude {get;set;}
        public float Latitude {get;set;}
        public string Name {get;set;}
        public bool Active {get;set;}
        public required string Action {get;set;}
        public int User_id {get;set;} 
    }
}