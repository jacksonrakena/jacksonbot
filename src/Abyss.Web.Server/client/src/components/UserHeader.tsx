import { Component } from "react";
import { PageHeader, Col, Tag, Row, Avatar } from "antd";
import * as React from 'react';

interface UserHeaderState extends React.ComponentState {
    data: object;
    tags: any;
}

const Description = ({ term, children, span = 12 }) => (
    <Col span={span}>
        <div className="description">
            <div className="term"><b>{term}</b></div>
            <div className="detail">{children}</div>
        </div>
    </Col>
);

export class UserHeader extends Component<object, UserHeaderState> {
    state = {
        data: {}, tags: [] };
    async componentWillMount() {
        var st = await fetch("api/status");
        var d = await st.json();
        console.log(d);
        this.setState({
            data: d,
            tags: [<Tag color="green">Online</Tag>]
        })
    }
    render() {
        var content = <Row>
            <Col span={4}><Avatar size={64} src={this.state.data['avatarUrl']} /></Col>
            <Description term="Guilds" span={2}>{this.state.data['guilds']}</Description>
            <Description term="Channels" span={2}>{this.state.data['channels']}</Description>
            <Description term="Commands" span={2}>{this.state.data['commands']}</Description>
        </Row>
        return <PageHeader title={this.state.data['serviceName']} subTitle={this.state.data['operatingSystem']} tags={this.state.tags}>
            <div className="wrap">
                <div className="content padding">{content}</div>
            </div>
        </PageHeader>;
    }
}