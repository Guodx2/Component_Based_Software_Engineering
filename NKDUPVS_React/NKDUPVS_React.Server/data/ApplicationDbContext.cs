using Microsoft.EntityFrameworkCore;
using NKDUPVS_React.Server.Models;
using Task = NKDUPVS_React.Server.Models.Task;

namespace NKDUPVS_React.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Mentor> Mentors { get; set; }
        public DbSet<Mentee> Mentees { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<UserClass> UserClasses { get; set; }
        public DbSet<RejectionHistory> RejectionHistories { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<MentorExpertise> MentorExpertises { get; set; }
        public DbSet<MentorRequest> MentorRequests { get; set; }
        public DbSet<StudyProgram> StudyPrograms { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<RequestStatus> RequestStatuses { get; set; }
        public DbSet<TaskType> TaskTypes { get; set; }
        public DbSet<SemesterPlan> SemesterPlans { get; set; }
        public DbSet<SemesterPlanTask> SemesterPlanTasks { get; set; }
        public DbSet<Feedback> Feedback { get; set; }
        public DbSet<Affair> Affairs { get; set; }
        public DbSet<Training> Trainings { get; set; }
        public DbSet<SemesterPlanEvent> SemesterPlanEvents { get; set; }
        public DbSet<Event> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure primary key for User
            modelBuilder.Entity<User>().HasKey(u => u.Code);

            // Configure primary key for Mentee
            modelBuilder.Entity<Mentee>().HasKey(m => m.Code);

            // Configure primary key for Mentor
            modelBuilder.Entity<Mentor>().HasKey(m => m.Code);

            // Configure primary key for Class
            modelBuilder.Entity<Class>().HasKey(c => c.Code);

            // Configure primary key for UserClass
            modelBuilder.Entity<UserClass>().HasKey(uc => uc.Id);

            // Map to the correct table name
            modelBuilder.Entity<User>().ToTable("user");
            modelBuilder.Entity<Mentee>().ToTable("mentee");
            modelBuilder.Entity<Mentor>().ToTable("mentor");
            modelBuilder.Entity<Class>().ToTable("class");
            modelBuilder.Entity<UserClass>().ToTable("userclass");
            modelBuilder.Entity<RejectionHistory>().ToTable("rejectionhistory");
            modelBuilder.Entity<MentorExpertise>().ToTable("mentorexpertise");
            modelBuilder.Entity<MentorRequest>()
                .ToTable("mentorrequest")
                .HasOne(mr => mr.RequestStatus)
                .WithMany()
                .HasForeignKey(mr => mr.RequestStatusId)
                .IsRequired();

            modelBuilder.Entity<MentorRequest>()
                .Property(mr => mr.RequestStatusId)
                .HasDefaultValue(1);

            modelBuilder.Entity<Mentee>().HasKey(m => m.Code);
            modelBuilder.Entity<Mentee>().ToTable("mentee");
            modelBuilder.Entity<Mentee>().Property(m => m.Code).HasColumnName("code");
            modelBuilder.Entity<Mentee>().Property(m => m.StudyProgram).HasColumnName("studyProgram");
            modelBuilder.Entity<Mentee>().Property(m => m.Specialization).HasColumnName("specialization");
            modelBuilder.Entity<Mentee>().Property(m => m.MentorCode).HasColumnName("MentorCode");

            modelBuilder.Entity<SemesterPlan>()
                  .HasMany(sp => sp.SemesterPlanEvents)
                  .WithOne()
                  .HasForeignKey(spe => spe.Fk_SemesterPlanid_SemesterPlan)
                  .HasConstraintName("FK_SemesterPlan_SemesterPlanEvent");

            modelBuilder.Entity<SemesterPlanEvent>(entity =>
            {
                  entity.ToTable("semesterplanevent");
                  entity.HasKey(e => e.id_SemesterPlanEvent);
                  entity.Property(e => e.id_SemesterPlanEvent).HasColumnName("id_SemesterPlanEvent");
                  entity.Property(e => e.Fk_SemesterPlanid_SemesterPlan).HasColumnName("fk_SemesterPlanid_SemesterPlan");
                  entity.Property(e => e.Fk_Eventid_Event).HasColumnName("fk_Eventid_Event");

                  // Explicitly configure the relationship to the Event entity.
                  entity.HasOne(e => e.Event)
                        .WithMany()
                        .HasForeignKey(e => e.Fk_Eventid_Event);
            });

            modelBuilder.Entity<Task>(entity =>
            {
                  entity.HasKey(t => t.Id_Task);
                  entity.ToTable("task");

                  entity.Property(t => t.Id_Task)
                          .HasColumnName("id_Task")
                          .ValueGeneratedOnAdd();

                  entity.Property(t => t.Name)
                          .HasColumnName("name");

                  entity.Property(t => t.Description)
                          .HasColumnName("description");

                  entity.Property(t => t.MaterialLink)
                          .HasColumnName("materialLink");

                  entity.Property(t => t.Deadline)
                          .HasColumnName("deadline");
                  
                  entity.Property(t => t.IsAssigned)
                          .HasColumnName("isAssigned");

                  entity.Property(t => t.CreatedBy)
                          .HasColumnName("createdBy");
            });

            modelBuilder.Entity<RejectionHistory>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.ToTable("rejectionhistory");

                entity.Property(r => r.UserCode)
                      .HasColumnName("userCode");

                entity.Property(r => r.Reason)
                      .HasColumnName("reason");

                entity.Property(r => r.RejectedAt)
                      .HasColumnName("rejectedAt");
            });

            // Configure property mappings for UserClass to match database column names
            modelBuilder.Entity<UserClass>(entity =>
            {
                entity.HasKey(uc => uc.Id);
                entity.ToTable("userclass");

                entity.Property(uc => uc.Id)
                      .HasColumnName("id_UserClass")
                      .ValueGeneratedOnAdd();

                entity.Property(uc => uc.UserCode)
                      .HasColumnName("fk_Usercode");

                entity.Property(uc => uc.ClassCode)
                      .HasColumnName("fk_Classcode");

                // Map extra fields to appropriate DB columns.
                entity.Property(uc => uc.Department).HasColumnName("department");
                entity.Property(uc => uc.Auditorium).HasColumnName("auditorium");
                entity.Property(uc => uc.StartTime).HasColumnName("startTime");
                entity.Property(uc => uc.EndTime).HasColumnName("endTime");
                entity.Property(uc => uc.Teacher).HasColumnName("teacher");
                entity.Property(uc => uc.Duration).HasColumnName("duration");
                entity.Property(uc => uc.Type).HasColumnName("type");

                entity.HasOne(uc => uc.User)
                      .WithMany()
                      .HasForeignKey(uc => uc.UserCode);

                entity.HasOne(uc => uc.Class)
                      .WithMany()
                      .HasForeignKey(uc => uc.ClassCode);
            });

            // Seed admin user
            modelBuilder.Entity<User>().HasData(new User
            {
                Code = "admin001",
                Username = "admin",
                Password = "admin123",
                Email = "admin@example.com",
                Name = "Admin",
                LastName = "User",
                PhoneNumber = "1234567890",
                IsAdmin = true
            });

            modelBuilder.Entity<Task>()
                .HasOne(t => t.TaskType)
                .WithMany()
                .HasForeignKey(t => t.TaskTypeId)
                .OnDelete(DeleteBehavior.SetNull); 

            modelBuilder.Entity<SemesterPlanTask>(entity =>
            {
                entity.ToTable("semesterplantask");
                entity.HasKey(e => e.Id_SemesterPlanTask);
                entity.Property(e => e.Id_SemesterPlanTask)
                        .HasColumnName("id_SemesterPlanTask")
                        .ValueGeneratedOnAdd();
                entity.Property(e => e.TaskId).HasColumnName("fk_Taskid_Task");
                entity.Property(e => e.SemesterPlanId).HasColumnName("fk_SemesterPlanid_SemesterPlan");
                entity.Property(e => e.CompletionFile).HasColumnName("completionFile");
                entity.Property(e => e.IsRated).HasColumnName("isRated");
                //entity.Property(e => e.Rating).HasColumnName("rating");
            });

            modelBuilder.Entity<SemesterPlan>(entity =>
            {
                entity.HasKey(sp => sp.Id_SemesterPlan);
                entity.ToTable("semesterplan");
                entity.Property(sp => sp.Id_SemesterPlan).HasColumnName("id_SemesterPlan");
                entity.Property(sp => sp.SemesterStartDate).HasColumnName("semesterStartDate");
                entity.Property(sp => sp.SemesterEndDate).HasColumnName("semesterEndDate");
                entity.Property(sp => sp.MentorCode).HasColumnName("fk_Mentorcode");
                entity.Property(sp => sp.MenteeCode).HasColumnName("fk_Menteecode");
            });

            modelBuilder.Entity<Feedback>(entity =>
            {
                entity.ToTable("feedback");

                // Map the key property to your existing column name.
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Id)
                      .HasColumnName("id_Feedback")
                      .ValueGeneratedOnAdd();

                entity.Property(f => f.SemesterPlanTaskId)
                      .HasColumnName("fk_SemesterPlanTask");

                // Explicitly specify the column type as int
                entity.Property(f => f.Rating)
                      .HasColumnName("rating")
                      .HasColumnType("int");  // Force EF to treat this as an integer

                entity.Property(f => f.Comment)
                      .HasColumnName("comment");

                entity.Property(f => f.SubmissionDate)
                      .HasColumnName("submissionDate")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP")
                      .ValueGeneratedOnAdd();

                entity.HasOne<SemesterPlanTask>()
                      .WithMany()
                      .HasForeignKey(f => f.SemesterPlanTaskId);
            });

            modelBuilder.Entity<Affair>().ToTable("affair");
            modelBuilder.Entity<Event>().ToTable("event");
            modelBuilder.Entity<Training>().ToTable("training");

            modelBuilder.Entity<Affair>().HasKey(a => a.id_Affair);
            modelBuilder.Entity<Event>().HasKey(e => e.id_Event);
            modelBuilder.Entity<Training>().HasKey(t => t.id_Training);

            modelBuilder.Entity<Affair>()
                  .HasOne(a => a.Event)
                  .WithMany()
                  .HasForeignKey(a => a.event_id)
                  .HasConstraintName("fk_affair_event");

            modelBuilder.Entity<Training>()
                  .HasOne(t => t.Event)
                  .WithMany()
                  .HasForeignKey(t => t.event_id)
                  .HasConstraintName("fk_training_event");

            modelBuilder.Entity<Training>(entity =>
            {
                  entity.HasKey(t => t.id_Training);
                  entity.Property(t => t.id_Training)
                        .ValueGeneratedOnAdd();
            });
        }
    }
}