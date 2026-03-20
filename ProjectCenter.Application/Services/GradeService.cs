using ProjectCenter.Application.DTOs.Grade;
using ProjectCenter.Application.Interfaces;
using ProjectCenter.Core.Entities;
using ProjectCenter.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCenter.Application.Services
{
    public class GradeService : IGradeService
    {
        private readonly IGradeRepository _gradeRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;

        public GradeService(
            IGradeRepository gradeRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository)
        {
            _gradeRepository = gradeRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
        }

        public async Task<GradeDto> SetGradeAsync(int teacherUserId, GradeRequestDto dto)
        {
            // 1. Получаем преподавателя
            var teacher = await _userRepository.GetByIdAsync(teacherUserId);
            if (teacher == null || teacher.Teacher == null)
                throw new Exception("Вы не преподаватель");

            // 2. Получаем проект
            var project = await _projectRepository.GetProjectByIdAsync(dto.ProjectId);
            if (project == null)
                throw new ProjectNotFoundException(dto.ProjectId);

            // ❗ 3. Проверка: это его студент?
            if (project.TeacherId != teacher.Teacher.Id)
                throw new ProjectAccessDeniedException();

            // 4. Проверка значения оценки
            if (dto.Value < 1 || dto.Value > 5)
                throw new ArgumentException("Оценка должна быть от 1 до 5");

            // 5. Есть ли уже оценка?
            var existing = await _gradeRepository.GetByProjectIdAsync(dto.ProjectId);

            if (existing != null)
            {
                existing.Value = dto.Value;
                existing.Comment = dto.Comment;
                await _gradeRepository.UpdateAsync(existing);

                return new GradeDto
                {
                    Value = existing.Value,
                    Comment = existing.Comment,
                    CreatedAt = existing.CreatedAt
                };
            }

            // 6. Создаём новую
            var grade = new Grade
            {
                ProjectId = dto.ProjectId,
                TeacherId = teacher.Teacher.Id,
                Value = dto.Value,
                Comment = dto.Comment
            };

            await _gradeRepository.AddAsync(grade);

            return new GradeDto
            {
                Value = grade.Value,
                Comment = grade.Comment,
                CreatedAt = grade.CreatedAt
            };
        }
    }
}
