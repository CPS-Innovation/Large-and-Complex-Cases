import { ReactNode, useMemo, createContext, useReducer } from "react";
import {
  mainStateReducer,
  initialState,
  MainStateActions,
  MainState,
} from "../reducers/mainStateReducer";

interface MainStateProviderProps {
  children: ReactNode;
}

interface MainStateContextProps {
  state: MainState;
  dispatch: React.Dispatch<MainStateActions>;
}

const MainStateContext = createContext<MainStateContextProps>({
  state: initialState,
  dispatch: () => null,
});

const MainStateProvider: React.FC<MainStateProviderProps> = ({ children }) => {
  const [state, dispatch]: [MainState, React.Dispatch<MainStateActions>] =
    useReducer(mainStateReducer, initialState);
  const contextValue = useMemo(
    () => ({
      state,
      dispatch,
    }),
    [state],
  );

  return (
    <MainStateContext.Provider value={contextValue}>
      {children}
    </MainStateContext.Provider>
  );
};

export { MainStateProvider, MainStateContext };
