using EVRentalSystem.Application.DTOs.Maintenance;
using EVRentalSystem.Application.Interfaces;
using EVRentalSystem.Domain.Entities;
using EVRentalSystem.Domain.Enums;
using EVRentalSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVRentalSystem.Infrastructure.Services;

public class MaintenanceService : IMaintenanceService
{
    private readonly ApplicationDbContext _context;

    public MaintenanceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MaintenanceScheduleResponse?> CreateScheduleAsync(int staffId, CreateMaintenanceScheduleRequest request)
    {
        // Validate vehicle exists
        var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
        if (vehicle == null)
        {
            return null;
        }

        // Validate maintenance type
        if (!Enum.IsDefined(typeof(MaintenanceType), request.Type))
        {
            return null;
        }

        var schedule = new MaintenanceSchedule
        {
            VehicleId = request.VehicleId,
            ScheduledDate = request.ScheduledDate,
            Type = (MaintenanceType)request.Type,
            Status = MaintenanceStatus.Scheduled,
            Notes = request.Notes,
            AssignedTechnicianId = request.AssignedTechnicianId,
            CreatedAt = DateTime.UtcNow
        };

        _context.MaintenanceSchedules.Add(schedule);
        await _context.SaveChangesAsync();

        return await MapScheduleToResponseAsync(schedule);
    }

    public async Task<MaintenanceScheduleResponse?> UpdateScheduleAsync(int staffId, UpdateMaintenanceScheduleRequest request)
    {
        var schedule = await _context.MaintenanceSchedules
            .Include(s => s.Vehicle)
            .FirstOrDefaultAsync(s => s.Id == request.ScheduleId);

        if (schedule == null)
        {
            return null;
        }

        // Validate status
        if (!Enum.IsDefined(typeof(MaintenanceStatus), request.Status))
        {
            return null;
        }

        schedule.Status = (MaintenanceStatus)request.Status;
        schedule.Notes = request.Notes ?? schedule.Notes;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await MapScheduleToResponseAsync(schedule);
    }

    public async Task<List<MaintenanceScheduleResponse>> GetVehicleSchedulesAsync(int vehicleId)
    {
        var schedules = await _context.MaintenanceSchedules
            .Include(s => s.Vehicle)
            .Where(s => s.VehicleId == vehicleId)
            .OrderByDescending(s => s.ScheduledDate)
            .ToListAsync();

        var responses = new List<MaintenanceScheduleResponse>();
        foreach (var schedule in schedules)
        {
            var response = await MapScheduleToResponseAsync(schedule);
            if (response != null)
            {
                responses.Add(response);
            }
        }

        return responses;
    }

    public async Task<List<MaintenanceScheduleResponse>> GetUpcomingSchedulesAsync(int? stationId = null)
    {
        var query = _context.MaintenanceSchedules
            .Include(s => s.Vehicle)
            .Where(s => s.Status == MaintenanceStatus.Scheduled && s.ScheduledDate >= DateTime.UtcNow);

        if (stationId.HasValue)
        {
            query = query.Where(s => s.Vehicle.StationId == stationId.Value);
        }

        var schedules = await query
            .OrderBy(s => s.ScheduledDate)
            .ToListAsync();

        var responses = new List<MaintenanceScheduleResponse>();
        foreach (var schedule in schedules)
        {
            var response = await MapScheduleToResponseAsync(schedule);
            if (response != null)
            {
                responses.Add(response);
            }
        }

        return responses;
    }

    public async Task<MaintenanceRecordResponse?> CreateRecordAsync(int technicianId, CreateMaintenanceRecordRequest request)
    {
        // Validate vehicle exists
        var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
        if (vehicle == null)
        {
            return null;
        }

        // Validate maintenance type
        if (!Enum.IsDefined(typeof(MaintenanceType), request.Type))
        {
            return null;
        }

        var record = new MaintenanceRecord
        {
            VehicleId = request.VehicleId,
            MaintenanceScheduleId = request.MaintenanceScheduleId,
            MaintenanceDate = DateTime.UtcNow,
            Type = (MaintenanceType)request.Type,
            Cost = request.Cost,
            Description = request.Description,
            TechnicianId = technicianId,
            CreatedAt = DateTime.UtcNow
        };

        _context.MaintenanceRecords.Add(record);

        // If this record is for a schedule, update schedule status to Completed
        if (request.MaintenanceScheduleId.HasValue)
        {
            var schedule = await _context.MaintenanceSchedules.FindAsync(request.MaintenanceScheduleId.Value);
            if (schedule != null)
            {
                schedule.Status = MaintenanceStatus.Completed;
                schedule.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        return await MapRecordToResponseAsync(record);
    }

    public async Task<List<MaintenanceRecordResponse>> GetVehicleRecordsAsync(int vehicleId)
    {
        var records = await _context.MaintenanceRecords
            .Include(r => r.Vehicle)
            .Where(r => r.VehicleId == vehicleId)
            .OrderByDescending(r => r.MaintenanceDate)
            .ToListAsync();

        var responses = new List<MaintenanceRecordResponse>();
        foreach (var record in records)
        {
            var response = await MapRecordToResponseAsync(record);
            if (response != null)
            {
                responses.Add(response);
            }
        }

        return responses;
    }

    private async Task<MaintenanceScheduleResponse?> MapScheduleToResponseAsync(MaintenanceSchedule schedule)
    {
        var vehicle = schedule.Vehicle ?? await _context.Vehicles.FindAsync(schedule.VehicleId);
        if (vehicle == null)
        {
            return null;
        }

        string? technicianName = null;
        if (schedule.AssignedTechnicianId.HasValue)
        {
            var technician = await _context.Users.FindAsync(schedule.AssignedTechnicianId.Value);
            technicianName = technician?.FullName;
        }

        return new MaintenanceScheduleResponse
        {
            Id = schedule.Id,
            VehicleId = schedule.VehicleId,
            VehicleLicensePlate = vehicle.LicensePlate,
            ScheduledDate = schedule.ScheduledDate,
            Type = schedule.Type.ToString(),
            Status = schedule.Status.ToString(),
            Notes = schedule.Notes,
            AssignedTechnicianId = schedule.AssignedTechnicianId,
            AssignedTechnicianName = technicianName,
            CreatedAt = schedule.CreatedAt
        };
    }

    private async Task<MaintenanceRecordResponse?> MapRecordToResponseAsync(MaintenanceRecord record)
    {
        var vehicle = record.Vehicle ?? await _context.Vehicles.FindAsync(record.VehicleId);
        if (vehicle == null)
        {
            return null;
        }

        var technician = await _context.Users.FindAsync(record.TechnicianId);
        if (technician == null)
        {
            return null;
        }

        return new MaintenanceRecordResponse
        {
            Id = record.Id,
            VehicleId = record.VehicleId,
            VehicleLicensePlate = vehicle.LicensePlate,
            MaintenanceScheduleId = record.MaintenanceScheduleId,
            MaintenanceDate = record.MaintenanceDate,
            Type = record.Type.ToString(),
            Cost = record.Cost,
            Description = record.Description,
            TechnicianId = record.TechnicianId,
            TechnicianName = technician.FullName,
            CreatedAt = record.CreatedAt
        };
    }
}

