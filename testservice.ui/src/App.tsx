import React from 'react'
import './App.css'
import { HashRouter as Router, Route, Switch } from 'react-router-dom';
import MainPage from './pages/MainPage';

const App = () => {

  return (
    <Router>
      <Switch>
        <Route path=''>
          <MainPage />
        </Route>
      </Switch>
    </Router>
  );
}

export default App;
