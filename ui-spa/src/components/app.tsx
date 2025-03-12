import Layout from "./layout";
import { Auth } from "../auth";
import { BrowserRouter } from "react-router";
import AppRoutes from "../components/routes";

import { MainStateProvider } from "../providers/mainStateProvider";

console.log("hii app");

function App() {
  return (
    <BrowserRouter>
      <Auth>
        <MainStateProvider>
          <Layout>
            <AppRoutes />
          </Layout>
        </MainStateProvider>
      </Auth>
    </BrowserRouter>
  );
}

export default App;
