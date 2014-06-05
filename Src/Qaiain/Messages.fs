namespace Grean.Qaiain

type Address = {
    SmtpAddress : string
    DisplayName : string
}

type EmailData = {
    From : Address
    To : Address array
    Subject : string
    Body : string
}

type EmailReference = {
    DataAddress : string
}