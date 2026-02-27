Azure Foundation Setup (Architect Mode)

Step 1 — Create Azure account properly

	-	Azure Portal (if Not)
		-	Search: Subscriptions
		-	Pay As You Go
		- 	Enter the following
			-	Profile info
			-	Identity verification
			-	Phone verification
			-	Credit card entry
		-	Select Support Information
			-	No Technical Support

	-	Configure MFA (Personal Microsoft Account)
		-	https://account.microsoft.com/security
		- 	Turn on Two-Step Verification if off
			-	Choose Microsoft Authenticator app
			- 	Add your phone number as backup method
			
	-	Why This Matters (Architect Mindset)
		-	As a Cloud AI Architect:
			-	You will use Azure CLI
			-	You will deploy IaC
			-	You will manage subscriptions
			-	You will hold privileged access
			-	dentity security is foundational.

		-	Architect Insight
			-	There’s a difference between:
				-	Having verification methods available vs Enforcing two-step verification globally
				-	Right now you have methods available, and we enforce it
				
			You are practicing real cloud hygiene now		
		
	-	Subscriptions
		- 	Add New Subscription
		- 	Name Subscription gio-architecture-lab
			- gio-architecture-lab
		
		
Step 2 - Set up cost control (budget + alerts)
	
	-	Add Budget
		- 	Budget Name
			- 	lab-monthly-limit
		- 	Budget Amount
			- 	$50
		- 	Budget Alerts
			-	Type → Select Actual
			- 	% of budget → Enter 50, 75, 90
			-	Leave Action group = None (email is enough for now)
		-	Alert recipients (email)
			-	Gio.Carozza@outlook.com
			
	- 	Architect Insight
		-	Financial discipline
		-	Consumption guardrails
		-	Early warning system
		-	Architect-level governance behavior
		
		You are now architecting with guardrails
		

Step 3 - Create Your First Resource Group

	-	Resource Group Strategy
		-	Naming Convention
			-	rg-{project}-{env}-{region}
			-	Examples:
				-	rg-ai-lab-dev-eastus
				-	rg-ai-lab-prod-eastus
				-	rg-mlops-test-eastus
			-	This shows:
				-	Project
				-	Environment
				-	Region

			This is how enterprise teams operate
			
	-	Tagging Strategy (CRITICAL for Real Architects)
		-	Tags allow cost tracking + governance.
		-	We create a mandatory tagging policy:
			-	Tag Name		Example Value
				Owner			Giovanni
				Project			AI-Lab
				Environment		Dev
				CostCenter		Personal-Learning
				Criticality		Low
		When you create any resource group: Add these tags immediately
		
	-	Create Your First Resource Group
		-	Resource Groups → Create
			-	Settings:
				-	Name: rg-ai-lab-dev-eastus
				-	Region: East US
				-	Add your tags

				This becomes your playground container and Everything goes inside this
				
	- 	Add Windows Defender Basic Plan
		-	register to Microsoft.Security if Subscription Not Registered
		-	Defender for Cloud → Environment settings
			-	Go to Environment Settings
				-	Select gio-architecture-lab
			This teaches you security posture awareness early.
			Security mindset = higher salary tier.
			

Recap - Architect-level

	-	Established governance
	-	Implemented cost control
	-	Created naming standards
	-	Applied tagging discipline
	-	Structured future environments

	This is architect behavior.

	-	Deliverable for Today
		-	Azure account created
		-	Budget alerts configured
		-	Naming convention written in Notion / README
		-	1 resource group created with tags


