import Layout from "./layout";
import { Auth } from "../auth";
import { BrowserRouter } from "react-router";
import AppRoutes from "../components/routes";

function App() {
  return (
    <BrowserRouter>
      <Auth>
        <Layout>
          <AppRoutes />
        </Layout>
      </Auth>
    </BrowserRouter>
  );
}

export default App;
