using EMIT.Application.DTOs;

namespace EMIT.Infrastructure.Services;

public class PdfService
{
    public string GenerateTimetableHtml(IEnumerable<ScheduleDto> schedules, string title)
    {
        var html = $@"<!DOCTYPE html>
<html lang=""fr"">
<head>
    <meta charset=""UTF-8"">
    <title>{title}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        h1 {{ color: #1a73e8; text-align: center; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        th {{ background-color: #1a73e8; color: white; }}
        tr:nth-child(even) {{ background-color: #f2f2f2; }}
        .footer {{ text-align: center; margin-top: 30px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <h1>{title}</h1>
    <table>
        <thead>
            <tr>
                <th>Jour</th>
                <th>Horaire</th>
                <th>Niveau</th>
                <th>Salle</th>
                <th>Enseignant</th>
                <th>Matière</th>
            </tr>
        </thead>
        <tbody>";

        foreach (var s in schedules.OrderBy(s => s.Day).ThenBy(s => s.StartTime))
        {
            html += $@"<tr>
                <td>{s.Day}</td>
                <td>{s.StartTime:hh\:mm} - {s.EndTime:hh\:mm}</td>
                <td>{s.Level}</td>
                <td>{s.RoomName}</td>
                <td>{s.TeacherName}</td>
                <td>{s.Subject}</td>
            </tr>";
        }

        html += @"</tbody></table>
    <div class=""footer"">EMIT University System - Généré le " + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + @"</div>
</body>
</html>";

        return html;
    }
}
