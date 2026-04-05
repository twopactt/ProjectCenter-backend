using ProjectCenter.Application.DTOs;
using ProjectCenter.Application.DTOs.Directory;

using ProjectCenter.Application.Interfaces;
using ProjectCenter.Core.Exceptions;

namespace ProjectCenter.Application.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ITeacherRepository _repo;
        private readonly IUserRepository _userRepository;

        public TeacherService(ITeacherRepository repo, IUserRepository userRepository)
        {
            _repo = repo;
            _userRepository = userRepository;
        }

        public async Task<List<TeacherDto>> GetAllTeachersAsync()
        {
            var teachers = await _repo.GetAllTeachersAsync();

            return teachers.Select(t => new TeacherDto
            {
                Id = t.Id,
                Surname = t.User.Surname,
                Name = t.User.Name,
                Patronymic = t.User.Patronymic
            }).ToList();
        }
        public async Task<List<StudentShortDto>> GetMyStudentsAsync(int userId)
        {
            var user = await _userRepository.GetFullUserByIdAsync(userId);

            if (user == null)
                throw new UserNotFoundException(userId);

            if (user.Teacher == null)
                throw new AccessDeniedException("Только преподаватель может просматривать студентов.");

            // 🔹 2. Получаем студентов
            var students = await _repo.GetStudentsByTeacherIdAsync(user.Teacher.Id);

            // 🔹 3. Фильтруем: только с проектами
            var result = students
                .Where(s => s.Projects != null && s.Projects.Any())
                .Select(s =>
                {
                    var project = s.Projects.First(); // у тебя 1 активный проект

                    return new StudentShortDto
                    {
                        Id = s.Id,
                        FullName = $"{s.User.Surname} {s.User.Name} {s.User.Patronymic}".Trim(),
                        GroupName = s.Group?.Name,
                        ProjectId = project.Id,
                        ProjectTitle = project.Title,
                        ProjectStatus = project.Status.Name,

                        Grade = project.Grade?.Value,
                        GradeComment = project.Grade?.Comment
                    };
                })
                .ToList();

            // 🔹 4. Если вообще нет студентов с проектами
            if (!result.Any())
                throw new ArgumentException("У ваших студентов пока нет проектов.");

            return result;
        }
    }
}
