using AutoMapper;

using ERP.Application.DTOs;
using ERP.Domain.Entities;

namespace ERP.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Project mappings
            CreateMap<Project, ProjectDto>()
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.Period.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.Period.EndDate))
                .ForMember(dest => dest.Budget, opt => opt.MapFrom(src => src.Budget.Amount))
                // Currency는 무조건 USD로 설정
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => "USD"))
                .ForMember(dest => dest.Progress, opt => opt.MapFrom(src => src.CalculateProgress()))
                .ForMember(dest => dest.IsOverBudget, opt => opt.MapFrom(src => src.IsOverBudget()))
                // Navigation Property null 체크
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src =>
                    src.Client != null ? src.Client.CompanyName : string.Empty))
                .ForMember(dest => dest.ManagerName, opt => opt.MapFrom(src =>
                    src.Manager != null ? src.Manager.Name : string.Empty))
                // 컬렉션 null 체크
                .ForMember(dest => dest.Technologies, opt => opt.MapFrom(src =>
                    src.Technologies ?? new List<string>()))
                .ForMember(dest => dest.Assignments, opt => opt.MapFrom(src =>
                    src.Assignments ?? new List<ProjectAssignment>()))
                .ForMember(dest => dest.Milestones, opt => opt.MapFrom(src =>
                    src.Milestones ?? new List<ProjectMilestone>()));

            CreateMap<ProjectAssignment, ProjectAssignmentDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src =>
                    src.Employee != null ? src.Employee.Name : string.Empty))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.Period.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.Period.EndDate));

            CreateMap<ProjectMilestone, ProjectMilestoneDto>();

            // Employee mappings
            CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.YearsOfService, opt => opt.MapFrom(src => src.GetYearsOfService()))
                .ForMember(dest => dest.CurrentAllocation, opt => opt.MapFrom(src => src.GetCurrentTotalAllocation()))
                .ForMember(dest => dest.IsOverallocated, opt => opt.MapFrom(src => src.IsOverallocated))
                .ForMember(dest => dest.PrimarySkills, opt => opt.MapFrom(src => src.PrimarySkills));

            CreateMap<EmployeeSkill, EmployeeSkillDto>();

            // Client mappings
            CreateMap<Client, ClientDto>()
                .ForMember(dest => dest.ClientImportance, opt => opt.MapFrom(src => src.GetClientImportance()));

            // TimeEntry mappings
            CreateMap<TimeEntry, TimeEntryDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src =>
                    src.Employee != null ? src.Employee.Name : string.Empty))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src =>
                    src.Project != null ? src.Project.Name : string.Empty))
                .ForMember(dest => dest.ProjectCode, opt => opt.MapFrom(src =>
                    src.Project != null ? src.Project.Code : string.Empty));
        }
    }
}