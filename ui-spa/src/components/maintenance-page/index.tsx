import {
  MAINTENANCE_MODE_MESSAGE1,
  MAINTENANCE_MODE_MESSAGE2,
} from "../../config";

const MaintenancePage = () => {
  const content1 =
    MAINTENANCE_MODE_MESSAGE1 ??
    "We're carrying out planned maintenance. The service will be unavailable.";
  const content2 = MAINTENANCE_MODE_MESSAGE2 ?? "Please try again later.";
  return (
    <div>
      <h1>Sorry this service is unavailable</h1>
      <p>{content1}</p>
      <p>{content2}</p>
      <p>If you need help, contact the product team in the Teams channel.</p>
    </div>
  );
};

export default MaintenancePage;
