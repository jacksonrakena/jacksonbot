import { Component } from 'react';
import * as React from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { SupportServerInfo } from './components/SupportServerInfo';

import './custom.css'

export default class App extends Component {
  render () {
      return (
          <Layout>
              <Route exact path='/' component={Home} />
              <Route path='/supportserver' component={SupportServerInfo} />
          </Layout>
    );
  }
}
