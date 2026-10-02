using System;
using System.Collections.Generic;
using System.Linq;
using AutoSphere.Models;

namespace AutoSphere.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        public ServiceInsight GetNextServiceInsight(User? user, List<Booking> bookings)
        {
            if (bookings == null || !bookings.Any())
            {
                return new ServiceInsight
                {
                    PredictiveDate = DateTime.Now.AddMonths(1),
                    Recommendation = "General Inspection & Diagnostic Health Check"
                };
            }

            var lastBooking = bookings.OrderByDescending(b => b.ScheduledDate).FirstOrDefault();
            var lastDate = lastBooking?.ScheduledDate ?? DateTime.Now;

            var nextDate = lastDate.AddMonths(3);
            if (nextDate < DateTime.Now)
            {
                nextDate = DateTime.Now.AddDays(7);
            }

            return new ServiceInsight
            {
                PredictiveDate = nextDate,
                Recommendation = "Periodic Maintenance & Comprehensive Fluid Check"
            };
        }
    }
}
