# ADR-002: Current Lab Environment Is Not a Full Landing Zone

## Status
Accepted

## Date
2026-03-16

## Context

The current Azure lab environment contains:

Subscription: gio-architecture-lab  
Resource Group: rg-ai-lab-dev-eastus  

This setup supports early learning activities but does not yet represent a full enterprise cloud platform.

Azure Cloud Adoption Framework describes landing zones as structured environments that include governance, identity, networking, and operational controls.

## Decision

Treat the current lab environment as a simplified learning environment rather than a full Azure landing zone.

## Rationale

The goal of the lab is to learn architecture concepts progressively without introducing unnecessary complexity too early.

## Consequences

### Positive

• simpler environment for experimentation  
• faster setup for learning exercises  

### Negative

• does not yet represent enterprise architecture patterns  

Future roadmap phases may introduce:

• governance policies  
• networking architecture  
• subscription segmentation  
• identity design