using Socigy.OpenSource.DB.Attributes;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Example.Auth.DB
{
    [Table("user_visibility")]
    public enum UserVisibility : short
    {
        [Description("This will make the user visible to everyone")]
        Public,
        CirclesOnly,
        CustomCircles
    }

    [Table("users")]
    [Check("LEN(email) < 25")]
    public partial class User
    {
        [PrimaryKey]
        public Guid ID { get; set; }

        public string Username { get; set; }
        public short Tag { get; set; }

        public string? IconUrl { get; set; }

        [StringLength(10)] // TOOD: Implement this
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        public bool RegistrationComplete { get; set; }

        public string? PhoneNumber { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public DateTime? BirthDate { get; set; }

        public bool IsChild { get; set; }
        public Guid? ParentId { get; set; }

        public UserVisibility Visibility { get; set; }
    }


    [Table("courses")]
    public partial class Course
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string Name { get; set; } = "DEFAULT NAME";

        [Default("timezone('utc', now())")]
        public DateTime CreatedAt { get; set; }
    }


    [Table("user_course")]
    public partial class UserCourse
    {
        [PrimaryKey, ForeignKey(typeof(User))]
        public Guid UserId { get; set; }
        [PrimaryKey, ForeignKey(typeof(Course))]
        public Guid CourseId { get; set; } // Test of type matching

        public DateTime RegisteredAt { get; set; }
    }

    [Table("user_course_agreement")]
    [ForeignKey(typeof(UserCourse), Keys = [nameof(UserId), nameof(CourseId)], TargetKeys = [nameof(UserCourse.UserId), nameof(UserCourse.CourseId)])]
    public partial class UserCourseAgreement
    {
        public Guid UserId { get; set; }
        public Guid CourseId { get; set; }
    }
}
