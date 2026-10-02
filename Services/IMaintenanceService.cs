using System;
using System.Collections.Generic;
using AutoSphere.Models;

namespace AutoSphere.Services
{
    public class ServiceInsight
    {
        public DateTime? PredictiveDate { get; set; }
        public string Recommendation { get; set; } = string.Empty;
    }

    public interface IMaintenanceService
    {
        ServiceInsight GetNextServiceInsight(User? user, List<Booking> bookings);
    }
}
