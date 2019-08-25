import { Component } from 'react';
import { Container } from 'reactstrap';
import { NavMenu } from './NavMenu';
import { UserHeader } from './UserHeader';
import * as React from 'react';

export class Layout extends Component {
    static displayName = "Layout";

  render () {
    return (
      <div>
        <UserHeader />
        <NavMenu />
        <Container>
          {this.props.children}
        </Container>
      </div>
    );
  }
}
