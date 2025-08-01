﻿namespace PRNFE.MVC.Models.Response
{
    // không được xóa// GET Show vehical
    public class VehicleResponses
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public DateTime CreateAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }


    // Model cho API GET /api/Vehicles/{id}
    public class DetailedVehicleResponses
    {
        public int Id { get; set; }
        public int ResidentId { get; set; }
        public int Type { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public ResidentListResponses Resident { get; set; } = new ResidentListResponses();
        public DateTime CreateAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
