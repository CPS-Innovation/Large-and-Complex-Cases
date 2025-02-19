variable "environment" {
  type = object({
    name  = string
    alias = string
  })
}

variable "location" {
  default = "UK South"
}

variable "terraform_service_principal_display_name" {
  type = string
}

variable "service_plans" {
  type = object({
    ui_service_plan_sku    = string
    api_service_plan_sku   = string
    proxy_service_plan_sku = string
  })
}

variable "dns_server" {
  type = string
}

variable "dns_alt_server" {
  type = string
}

variable "cms_details" {
  type = object({
    default_upstream_cms_ip_corsham            = string
    default_upstream_cms_modern_ip_corsham     = string
    default_upstream_cms_ip_farnborough        = string
    default_upstream_cms_modern_ip_farnborough = string
    default_upstream_cms_domain_name           = string
    default_upstream_cms_modern_domain_name    = string
    default_upstream_cms_services_domain_name  = string
    cin2_upstream_cms_ip_corsham               = string
    cin2_upstream_cms_modern_ip_corsham        = string
    cin2_upstream_cms_ip_farnborough           = string
    cin2_upstream_cms_modern_ip_farnborough    = string
    cin2_upstream_cms_domain_name              = string
    cin2_upstream_cms_modern_domain_name       = string
    cin2_upstream_cms_services_domain_name     = string
    cin4_upstream_cms_ip_corsham               = string
    cin4_upstream_cms_modern_ip_corsham        = string
    cin4_upstream_cms_ip_farnborough           = string
    cin4_upstream_cms_modern_ip_farnborough    = string
    cin4_upstream_cms_domain_name              = string
    cin4_upstream_cms_modern_domain_name       = string
    cin4_upstream_cms_services_domain_name     = string
    cin5_upstream_cms_ip_corsham               = string
    cin5_upstream_cms_modern_ip_corsham        = string
    cin5_upstream_cms_ip_farnborough           = string
    cin5_upstream_cms_modern_ip_farnborough    = string
    cin5_upstream_cms_domain_name              = string
    cin5_upstream_cms_modern_domain_name       = string
    cin5_upstream_cms_services_domain_name     = string
  })
}

variable "complex_cases_ui_sub_folder" {
  type = string
  // this value must match the PUBLIC_URL=... value
  //  as seen in the ui project top-level package.json
  //  scripts section.
  default = "complex-cases-ui"
}