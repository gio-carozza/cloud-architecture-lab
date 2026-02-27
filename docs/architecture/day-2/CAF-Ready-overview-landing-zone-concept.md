  CAF “Ready” overview + landing zone concept

-	Microsoft Cloud Adoption Framework (CAF)
	-	By Microsoft for Microsoft Azure
	-	CAF is Microsoft’s enterprise blueprint for cloud transformation.
	-	It has 5 major phases:
		-	Strategy
		-	Plan
		-	Ready
		-	Adopt
		-	Govern & Manage

- 	What Is the CAF “Ready” Phase?

	-	Simple Definition
		-	The Ready phase prepares your Azure environment before any real workloads are deployed.
		-	It is where you design and build your Azure landing zone.
			-	No apps yet.
			-	No AI models yet.
			-	No production systems yet.
			-	Just the foundation.
			
	-	CAF “Ready” = Build the Cloud Foundation
		-	Think of it like preparing land before building a city.
		-	You define:
			-	Identity model
			-	Subscription structure
			-	Network topology
			-	Security baselines
			-	Governance policies
			-	Management model
		-	Only after this is done can you safely deploy workloads.
		
	- 	The Landing Zone Concept
		-	Official Architect-Level Definition
			-	A landing zone is a scalable, secure Azure environment configured to host workloads according to organizational governance, security, and operational requirements.
		-	The Ready phase delivers this landing zone.
		
	- 	What CAF “Ready” Actually Includes
		-	Enterprise Design Areas
		-	CAF defines critical architecture domains you must design:
			-	Identity
			-	Uses:
				-	Microsoft Entra ID
				-	RBAC
				-	Conditional Access
				-	MFA
	
	- 	Management Group & Subscription Design
		-	Defines:
			-	Root management group
			-	Platform subscriptions
			-	Landing zone subscriptions
			-	Sandbox subscriptions
		-	Enterprise-scale org structure
		
	-	Networking Topology
		-	Includes:
			-	Hub-and-spoke
			-	Centralized firewall
			-	Private DNS
			-	Hybrid connectivity (VPN/ExpressRoute)
			
	- 	Security Baseline
		-	Uses:
			-	Microsoft Defender for Cloud
			-	Azure Policy
			-	Encryption standards
			-	Logging & monitoring
			
	- 	Governance
		-	Naming standards
		-	Tagging standards
		-	Budget controls
		-	Policy assignments
		
	- 	Two Landing Zone Approaches in CAF
		-	Enterprise-Scale Landing Zone
			-	Large organizations
			-	Multiple subscriptions
			-	Full governance
			-	Platform team + App teams
			-	This is what banks, healthcare, Fortune 500 use.
		-	Small/Medium Landing Zone
			-	Simpler:
				-	Few subscriptions
				-	Basic policy enforcement
				-	Simplified network
				
	-	Why “Ready” Matters (Architect Thinking)
		-	Most cloud failures happen because companies:
			❌ Deploy workloads first
			❌ Then try to add governance later
		-	CAF flips that:
			✅ Build foundation first
			✅ Then deploy workloads safely
			
	-	How It Connects to the Bigger CAF Flow
		-	Phase			What Happens
			Strategy		Define business goals
			Plan			Assess current environment
			Ready			Build landing zone
			Adopt			Migrate or build workloads
			Govern			Ensure compliance
			Manage			Operate & optimize

		-	Landing zone = bridge between planning and execution.
		
	-	Interview-Level Explanation
		-	If asked:
			-	“What is CAF Ready and how does it relate to landing zones?”
				-	You say:
					-	The CAF Ready phase focuses on establishing a secure, governed Azure landing zone aligned to enterprise architecture design areas including identity, 
						networking, security, and management. It ensures workloads are deployed into a compliant, scalable foundation rather than unmanaged cloud resources.

		-	That’s architect-level.
		
-	Since you're building toward AI Cloud Architect:
	-	Before you deploy:
		-	Azure OpenAI
		-	ML pipelines
		-	AKS clusters
		-	APIs
		-	Data platforms
	-	You must know:
		-	Where does it live?
		-	Who can access it?
		-	How is it monitored?
		-	How is it secured?
	-	What policies apply?
		-	That’s CAF Ready thinking.
		

-	Questions

	-	What Is a Landing Zone?
		-	Simple Definition
			-	A landing zone is a pre-configured Azure environment designed to securely host workloads.
			-	Built inside Microsoft Azure using best practices from the Microsoft Cloud Adoption Framework.
			-	It defines:
				-	Identity
				-	Network
				-	Security
				-	Governance
				-	Subscription structure
				-	Monitoring
				-	Cost controls

		-	Architect-Level Definition
			-	A landing zone is a governed Azure environment that establishes identity, networking, security, and operational baselines to enable compliant and scalable workload deployment.

			Think:
				-	Not the app.
				-	Not the AI model.
				-	Not the VM.

			-	It’s the ground they stand on.
			-	Without it → cloud chaos.
			
	- 	Why Management Groups Matter?
	
		-	This is where most beginners underestimate architecture.
		-	What Are Management Groups?
			-	They sit above subscriptions and allow centralized governance.

		-	Hierarchy example:
		
				Tenant Root
				 ├── Platform
				 ├── Production
				 ├── Non-Production
		
		-	Why They Matter
			-	Policy Inheritance
				-	If you assign a policy at the management group level:
				-	Every subscription below inherits it.
				-	No manual duplication.
			
			-	RBAC at Scale
				-	You can assign roles at:
				-	Management group level
				-	Subscription level
				-	Resource group level
				-	Enterprise architects design this hierarchy carefully.
				
			- 	Governance Consistency
				-	Without management groups:
					❌ Every subscription becomes its own island
					❌ Security differs everywhere
					❌ Cost controls differ
					❌ Naming standards differ

				-	With them:
					✅ Centralized control
					✅ Predictable structure
					✅ Enterprise consistency
					
			-	Architect Insight
				-	If a company has 100+ subscriptions and no management group strategy?
					-	It’s not a cloud strategy.
					-	It’s cloud sprawl.
					
	- 	Why Policies Prevent Chaos?
		
		-	Azure Policy is the guardrail system.

		-	What Policies Do
			-	They can:
				-	Deny resource creation
				-	Enforce tags
				-	Restrict regions
				-	Require encryption
				-	Enforce SKU types
				-	Auto-remediate misconfigurations

		-	Example Without Policies
			-	Developer deploys:
				-	Public storage account
				-	No encryption
				-	Wrong region
				-	No tags

			-	Now:
				-	Security risk
				-	Compliance violation
				-	Cost tracking broken
				-	Audit failure

		-	Example With Policies
			-	Policy says:
				-	Only East US allowed
				-	Must use private endpoints
				-	Must include cost-center tag
				-	Encryption required

			-	Result:
				-	Azure blocks non-compliant resources automatically.

		-	Architect Reality
			-	Policies = preventive control
			-	Not reactive cleanup.
			-	You don’t fix chaos.
			-	You prevent it.
			
	-	How Networking Fits Inside Landing Zones?
		
		-	Networking is the spine of the landing zone.
		-	If identity is the brain, networking is the nervous system.
		
		-	Typical Enterprise Networking Model
		
			-	Hub-and-Spoke Architecture
				-	Hub
					-	Azure Firewall
					-	VPN Gateway
					-	ExpressRoute
					-	DNS
					-	Shared services

				-	Spokes
					-	Application VNets
					-	Workload environments
					-	Isolated from each other
					
		-	Why Networking Is Designed in the Landing Zone
			-	Because you must define:
				-	Are resources public or private?
				-	Is internet access allowed?
				-	How does hybrid connect?
				-	Where does inspection occur?
				-	How do environments isolate?
			-	If you deploy networking after workloads…
			-	You rebuild everything later.
			
		-	How All Four Connect
			-	Component				Role in Landing Zone
				Management Groups		Governance structure
				Policies				Guardrails
				Networking				Secure connectivity
				Landing Zone			The complete governed foundation
				
		-	Architect Mindset Shift
			-	Junior thinking:
				-	"How do I deploy this app?"
			-	Architect thinking:
				-	"Where does this app live inside a governed, secure, scalable structure?"
			-	That shift is what gets you to $200k+ roles.
