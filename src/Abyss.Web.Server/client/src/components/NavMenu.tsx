import { Component } from 'react';
import { Link, Route } from 'react-router-dom';
import './NavMenu.css';
import * as React from 'react';
import { Menu, Icon } from 'antd';
import { ClickParam } from 'antd/lib/menu';
import { Home } from './Home';
import { SupportServerInfo } from './SupportServerInfo';

export class NavMenu extends Component {
    static displayName = "NavMenu";
    state = {
        currentTab: 'home'
    };

    constructor(props) {
        super(props);
        this.handleClick = this.handleClick.bind(this);
    }

    handleClick(param: ClickParam) {
        this.setState({
            currentTab: param.key
        });
    }

    render() {
        return (
            <Menu
                onClick={this.handleClick}
                selectedKeys={[this.state.currentTab]}
                mode="horizontal">

                <Menu.Item key="home">
                    <Link to="/">Home</Link>
                </Menu.Item>
                <Menu.Item key="supportserver">
                    <Link to="/supportserver">Support server</Link>
                </Menu.Item>
            </Menu>
        );
    }
}
