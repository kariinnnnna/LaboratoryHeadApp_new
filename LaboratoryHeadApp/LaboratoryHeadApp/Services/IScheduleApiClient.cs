using ScheduleServiceContracts.BindingModels;
using ScheduleServiceContracts.ViewModels;

public interface IScheduleApiClient
{
    Task<List<ScheduleItemViewModel>?> GetScheduleAsync();
    Task ImportGroupSchedulesFromFolderAsync(UniversityScheduleImportFolderBindingModel model);

    Task<List<LessonTimeViewModel>?> GetLessonTimesAsync();
    Task<LessonTimeViewModel?> GetLessonTimeByIdAsync(int id);
    Task<LessonTimeViewModel?> CreateLessonTimeAsync(LessonTimeBindingModel model);
    Task<LessonTimeViewModel?> UpdateLessonTimeAsync(LessonTimeBindingModel model);
    Task<bool> DeleteLessonTimeAsync(int id);

    Task<List<DutyScheduleViewModel>?> GetDutyScheduleAsync();

    Task<List<DutyPersonViewModel>?> GetDutyPersonsAsync();
    Task<DutyPersonViewModel?> GetDutyPersonByIdAsync(int id);
    Task<DutyPersonViewModel?> CreateDutyPersonAsync(DutyPersonBindingModel model);
    Task<DutyPersonViewModel?> UpdateDutyPersonAsync(DutyPersonBindingModel model);
    Task<bool> DeleteDutyPersonAsync(int id);

    Task<DutyScheduleViewModel?> CreateDutyScheduleAsync(DutyScheduleBindingModel model);
    Task<bool> DeleteDutyScheduleAsync(int id);
    Task<DutyScheduleViewModel?> GetDutyScheduleByIdAsync(int id);
    Task<DutyScheduleViewModel?> UpdateDutyScheduleAsync(DutyScheduleBindingModel model);
    Task<ScheduleItemViewModel?> CreateScheduleItemAsync(ScheduleItemBindingModel model);

    Task<List<GroupViewModel>?> GetGroupsAsync();
    Task<List<TeacherViewModel>?> GetTeachersAsync();

    Task<TeacherViewModel?> GetTeacherAsync(int id);
    Task<bool> UpdateTeacherAsync(TeacherBindingModel model);
    Task<bool> DeleteTeacherAsync(int id);
    Task<bool> ImportTeachersFromCoreAsync();

    Task<GroupViewModel?> GetGroupAsync(int id);
    Task<bool> UpdateGroupAsync(GroupBindingModel model);
    Task<bool> DeleteGroupAsync(int id);
    Task<bool> ImportGroupsFromCoreAsync();
    Task<bool> ImportAllFromCoreAsync();


}