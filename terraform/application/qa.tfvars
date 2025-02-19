environment = {
  name  = "qa"
  alias = "qa"
}

terraform_service_principal_display_name = "Azure Pipeline: Complex-Cases-QA"
dns_server                               = "10.7.197.20"
dns_alt_server                           = "168.63.129.16"

service_plans = {
  ui_service_plan_sku    = "P1mv3"
  api_service_plan_sku   = "P1mv3"
  proxy_service_plan_sku = "P1mv3"
}

cms_details = {
  // for non-prod environments, current thinking is to try to go to Corsham's IP
  //  even if we detect a farnborough cookie
  default_upstream_cms_ip_corsham            = "10.2.177.14"
  default_upstream_cms_modern_ip_corsham     = "10.2.177.67"
  default_upstream_cms_ip_farnborough        = "10.3.177.14"
  default_upstream_cms_modern_ip_farnborough = "10.3.177.67"
  default_upstream_cms_domain_name           = "cin3.cps.gov.uk"
  default_upstream_cms_modern_domain_name    = "cmsmodcin3.cps.gov.uk"
  default_upstream_cms_services_domain_name  = "not-used-in-cin3.cps.gov.uk"
  cin2_upstream_cms_ip_corsham               = "10.2.177.3"
  cin2_upstream_cms_modern_ip_corsham        = "10.2.177.67"
  cin2_upstream_cms_ip_farnborough           = "10.3.177.3"
  cin2_upstream_cms_modern_ip_farnborough    = "10.3.177.67"
  cin2_upstream_cms_domain_name              = "cin2.cps.gov.uk"
  cin2_upstream_cms_modern_domain_name       = "cmsmodcin2.cps.gov.uk"
  cin2_upstream_cms_services_domain_name     = "not-used-in-cin2.cps.gov.uk"
  cin4_upstream_cms_ip_corsham               = "10.2.177.35"
  cin4_upstream_cms_modern_ip_corsham        = "10.2.177.67"
  cin4_upstream_cms_ip_farnborough           = "10.3.177.35"
  cin4_upstream_cms_modern_ip_farnborough    = "10.3.177.67"
  cin4_upstream_cms_domain_name              = "cin4.cps.gov.uk"
  cin4_upstream_cms_modern_domain_name       = "cmsmodstage.cps.gov.uk"
  cin4_upstream_cms_services_domain_name     = "not-used-in-cin4.cps.gov.uk"
  cin5_upstream_cms_ip_corsham               = "10.2.177.21"
  cin5_upstream_cms_modern_ip_corsham        = "10.2.177.67"
  cin5_upstream_cms_ip_farnborough           = "10.3.177.21"
  cin5_upstream_cms_modern_ip_farnborough    = "10.3.177.67"
  cin5_upstream_cms_domain_name              = "cin5.cps.gov.uk"
  cin5_upstream_cms_modern_domain_name       = "cmsmodcin5.cps.gov.uk"
  cin5_upstream_cms_services_domain_name     = "not-used-in-cin5.cps.gov.uk"
}
