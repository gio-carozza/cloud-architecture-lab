# Resources — Deploy and manage Azure compute resources

## Day 007 additions (2026-06-03)

*Focus: App Service configuration and settings management*

---

## Official Microsoft Resources

- [Configure an App Service app](https://learn.microsoft.com/en-us/azure/app-service/configure-common) — App Service application settings, connection strings, and general settings; covers naming conventions and environment variable injection
- [App Service deployment best practices](https://learn.microsoft.com/en-us/azure/app-service/deploy-best-practices) — covers deployment slots, zip deploy, and run-from-package
- [ARM REST API: Web Apps - Update Application Settings](https://learn.microsoft.com/en-us/rest/api/appservice/web-apps/update-application-settings) — the PUT endpoint reference; note full-replace semantics
- [Use Key Vault references for App Service](https://learn.microsoft.com/en-us/azure/app-service/app-service-key-vault-references) — the production-grade secrets pattern beyond plain application settings
- [Kudu zip deploy](https://learn.microsoft.com/en-us/azure/app-service/deploy-zip) — covers WEBSITE_RUN_FROM_PACKAGE and zip deployment mechanics

## Diagrams and Visual References

- [App Service configuration hierarchy diagram](https://learn.microsoft.com/en-us/azure/app-service/configure-common#configure-app-settings) — MS Learn; shows how settings override appsettings.json
- [Azure App Service Architecture overview](https://learn.microsoft.com/en-us/azure/app-service/overview) — MS Learn; context for where application settings sit in the App Service model

## Video (≤ 20 min)

- [Azure App Service: Configuration and Deployment Slots](https://learn.microsoft.com/en-us/shows/exam-readiness-zone/preparing-for-az-104-deploy-and-manage-azure-compute-resources-3-of-5) — Exam Readiness Zone, ~15 min, covers AZ-104 domain 3 including App Service configuration
- [Azure App Service Deep Dive](https://www.youtube.com/watch?v=4BwyqmRTrx8) — Azure Fridays; covers App Service internals including settings injection

## Practice Assessment

- [AZ-104 free practice assessment](https://learn.microsoft.com/en-us/credentials/certifications/exams/az-104/practice/assessment?assessment-type=practice&assessmentId=21) — official MS Learn practice questions; Domain 3 questions will appear
