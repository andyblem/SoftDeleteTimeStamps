# SoftDeleteTimeStamps
An extension for entity-framework, that allows you to put timestamps on data when you do CRUD operations on it. It also add logic to do soft and hard deletes on data.

## Installation
Import the files in your project. You can change the namespaces and locations depending on your folder structure
Inherit from AuditableDbContext inside your db context. Make sure you have called the super constructor.
```
public class ApplicationDbContext : AuditableDbContext
{
    public ApplicationDbContext(
        IAuthenticatedUserService authenticatedUser,
        IDateTimeService dateTime,
        DbContextOptions<ApplicationDbContext> options)
        : base(authenticatedUser, dateTime, options)
    { }
}
```

Implement the IAuditable interface on your models

```
public class CustomModel : IAuditable
{
    public bool? IsDeleted { get; set; }
    public bool? IsModified { get; set; }
    public bool? IsRestored { get; set; }

    public int Id { get; set; }
    public int? OldId { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? RestoredAt { get; set; }


    public string? CreatedById { get; set; }
    public string? DeletedById { get; set; }
    public string? ModifiedById { get; set; }
    public string? RestoredById { get; set; }
}
```

You can create a base model that you can inherit on you custom models
```
public abstract class BEntity : IAuditable
{
    public bool? IsDeleted { get; set; }
    public bool? IsModified { get; set; }
    public bool? IsRestored { get; set; }

    public int Id { get; set; }
    public int? OldId { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? RestoredAt { get; set; }


    public string? CreatedById { get; set; }
    public string? DeletedById { get; set; }
    public string? ModifiedById { get; set; }
    public string? RestoredById { get; set; }
}
```
```
public class CustomModel : BEntity
{
    // custom fields
}
```

Extra configurations and implementions of your IAuthenticatedUserService and IDateService is required.
## Usage
With the above setup, Entity framework will automatically set the audit fields when you do CRUD operations on your data.
Delete operations will set the IsDeleted, DeletedBy and DeletedAt fields.
The same goes for create and update operations.
Entity framework will also automatically ignore records with the deleted data set when doing read and write operations on them.

Example to delete data
```
// soft deletes record
dbContext.Remove(<ObjectType>);

// hard deletes record
dbContext.Remove(<ObjectType>, false);
```

Example to restore data
```
dbContext.Restore(<ObjectType>);
```
