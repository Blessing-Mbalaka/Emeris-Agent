using EmerisAcademicSuccess.Domain.Entities;
using EmerisAcademicSuccess.Infrastructure.Services;

namespace EmerisAcademicSuccess.Tests;

public class UnitTest1
{
    [Fact]
    public void Planner_creates_reminder_action_when_request_contains_due_date()
    {
        var planner = new HeuristicAgentPlanner();

        var result = planner.CreatePlan(
            "Remind me to submit the database assignment on 25/09/2026 18:00",
            new DateTime(2026, 9, 22, 9, 0, 0));

        var action = Assert.Single(result.Actions, item => item.ToolName == "create_reminder");
        Assert.Equal("submit the database assignment", action.Arguments["title"]);
        Assert.Equal("2026-09-25 18:00", action.Arguments["dueAt"]);
    }

    [Fact]
    public void Timetable_parser_extracts_rows_from_word_style_text()
    {
        var parser = new ScheduleHeuristicParser();
        const string extractedText = "Monday 08:00 - 10:00 CSC101 Programming Lab Room A\nWednesday 13:00 - 15:00 MAT202 Calculus Lecture Hall 3";

        var entries = parser.Parse(DocumentCategory.Timetable, extractedText);

        Assert.Equal(2, entries.Count);
        Assert.Equal(DayOfWeek.Monday, entries[0].DayOfWeek);
        Assert.Equal("CSC101", entries[0].ModuleCode);
        Assert.Equal(new TimeOnly(8, 0), entries[0].StartTime);
    }
}
