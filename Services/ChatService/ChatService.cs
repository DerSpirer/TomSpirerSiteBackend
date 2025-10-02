using TomSpirerSiteBackend.Models;
using TomSpirerSiteBackend.Models.DTOs;
using TomSpirerSiteBackend.Services.ChatCompletionService;

namespace TomSpirerSiteBackend.Services.ChatService;

public class ChatService : IChatService
{
    private const string _professionalSummary =
@"# Professional Summary

I am a full-stack/backend engineer and tech lead with a lifelong passion for technology,
starting from building games in childhood to leading large-scale AI initiatives in production environments.
With over 4 years of professional experience at Glassix,
including 3 years as a software engineer and 1 year as a tech lead, I have driven the design, development,
and deployment of mission-critical backend and AI-driven systems that now power over 30% of Israel’s customer-service market.

## Career Journey

### Early Development & Education:

- Began programming in elementary school with Scratch and GameMaker Studio, later moving to Unity (C#) and participating in game jams.
- In high school, studied Physics and Computer Science (Java + Android), publishing a Google Play app that implemented version-control-like functionality for collaborative storytelling.
- Selected for Magshimim, a prestigious Israeli program preparing gifted students for careers in technology and cybersecurity (8200 unit track). There, I gained accelerated exposure to:
  - Computer Networks (OSI 5-layer model, HTTP, TCP, UDP, SMTP, HTTPS).
  - Cybersecurity (DoS/DDoS, MITM, SQL injection, encryption, assembly, C, C++, and data security concepts).
  - Server Development with Python and low-level system programming.

### First Professional Role (High School):

- Remote developer at Shapescape (Hamburg, Germany), building educational experiences in Minecraft using JavaScript and custom scripting.
- Designed gameplay logic, custom entity behavior, and narrative-driven experiences.

### Glassix (Current Role):

- Joined as a Software Engineer, promoted to Tech Lead after three years.
- Led the creation of the company’s AI Suite, developed almost entirely independently:
  - Automated AI agent responses.
  - Knowledge base management.
  - AI voice agents.
  - Sentiment analysis & classification models for tagging.
- Additional projects:
  - A custom logging system tailored for AI workflows, enabling real-time message editing, agent settings management, and live monitoring.
  - A function-writing system with code editor and AWS Lambda deployment support.
- Current responsibilities include leading a small team (2 engineers), conducting code reviews, mentoring/onboarding, and architecting scalable solutions.
- Recognized as a top contributor: CTO praised me as “the best backend developer I’ve ever seen, with only 2 years of experience.”

## Technical Skills

- Languages & Frameworks: C#, .NET, ASP.NET Core, JavaScript, React, Python, C, C++, Java, SQL.
- Cloud & DevOps: Azure (Blob, Queues, Tables, Service Bus, App Insights), AWS (Lambda, S3), CI/CD pipelines, microservices architecture.
- Databases: SQL Server, MongoDB, Redis, Elasticsearch.
- AI & ML Tools: OpenAI, Claude, ElevenLabs, sentiment analysis, classification models.
- Other: Game dev with Unity/C#.

## Leadership & Soft Skills

- Team Leadership: Tech lead for 1+ years, managing 2 engineers.
- Mentorship: Onboarding and guiding junior developers, running code reviews.
- Adaptability: Self-taught in multiple domains, quick learner with broad technical foundation.
- Recognition: Retained by employer with exceptional offers due to critical impact.

## Relocation & Availability

- Based in Israel, holding EU citizenship with full work authorization in Switzerland.
- Open to onsite or hybrid roles.
- Available to relocate within 1–2 months.

## Personal Interests

- Passionate about languages (conversational Portuguese, intermediate French).
- Music enthusiast: piano player and music producer.
- Strong independent learning drive, exploring new domains with curiosity and creativity.";
    
    private const string _systemMessage = @$"You are a helpful assistant that can answer questions about the user's professional summary.
Be helpful and professional in your responses.
If you don't know the answer, just say you don't know. Do not make up an answer.

The user's professional summary is:
```
{_professionalSummary}
```";

    private readonly IChatCompletionService _chatCompletionService;
    public ChatService(IChatCompletionService chatCompletionService)
    {
        _chatCompletionService = chatCompletionService;
    }

    public async Task<ServiceResult<Message>> GenerateResponse(GenerateResponseRequest request)
    {
        return await _chatCompletionService.GenerateResponse([
            new Message { role = Message.Role.system, content = _systemMessage },
            ..request.messages,
        ]);
    }
}