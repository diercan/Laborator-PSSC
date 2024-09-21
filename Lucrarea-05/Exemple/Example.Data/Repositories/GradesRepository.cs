using Example.Data.Models;
using Examples.Domain.Models;
using Examples.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Examples.Domain.Models.Exam;

namespace Example.Data.Repositories
{
  public class GradesRepository : IGradesRepository
  {
    private readonly GradesContext dbContext;

    public GradesRepository(GradesContext dbContext)
    {
      this.dbContext = dbContext;
    }

    public async Task<List<CalculatedStudentGrade>> GetExistingGradesAsync()
    {
      //load entities from database
      var foundStudentGrades = await (
        from g in dbContext.Grades
        join s in dbContext.Students on g.StudentId equals s.StudentId
        select new { s.RegistrationNumber, g.GradeId, g.Exam, g.Activity, g.Final }
      ).AsNoTracking()
       .ToListAsync();

      //map database entity to domain model
      List<CalculatedStudentGrade> foundGradesModel = foundStudentGrades.Select(result =>
        new CalculatedStudentGrade(
          StudentRegistrationNumber: new StudentRegistrationNumber(result.RegistrationNumber),
          ExamGrade: new Grade(result.Exam ?? 0m),
          ActivityGrade: new Grade(result.Activity ?? 0m),
          FinalGrade: new Grade(result.Final ?? 0m))
        {
          GradeId = result.GradeId
        })
         .ToList();

      return foundGradesModel;
    }

    public async Task SaveGradesAsync(PublishedExam exam)
    {
      ILookup<string, StudentDto> students = (await dbContext.Students.ToListAsync())
        .ToLookup(student => student.RegistrationNumber);
      AddNewGrades(exam, students);
      UpdateExistingGrades(exam, students);
      await dbContext.SaveChangesAsync();
    }

    private void UpdateExistingGrades(PublishedExam exam, ILookup<string, StudentDto> students)
    {
      IEnumerable<GradeDto> updatedGrades = exam.GradeList.Where(g => g.IsUpdated && g.GradeId > 0)
        .Select(g => new GradeDto()
        {
          GradeId = g.GradeId,
          StudentId = students[g.StudentRegistrationNumber.Value].Single().StudentId,
          Exam = g.ExamGrade.Value,
          Activity = g.ActivityGrade.Value,
          Final = g.FinalGrade.Value,
        });

      foreach (GradeDto entity in updatedGrades)
      {
        dbContext.Entry(entity).State = EntityState.Modified;
      }
    }

    private void AddNewGrades(PublishedExam exam, ILookup<string, StudentDto> students)
    {
      IEnumerable<GradeDto> newGrades = exam.GradeList
        .Where(g => !g.IsUpdated && g.GradeId == 0)
        .Select(g => new GradeDto()
        {
          StudentId = students[g.StudentRegistrationNumber.Value].Single().StudentId,
          Exam = g.ExamGrade.Value,
          Activity = g.ActivityGrade.Value,
          Final = g.FinalGrade.Value,
        });
      dbContext.AddRange(newGrades);
    }
  }
}
