using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using UserManagement.Domain.ValueObjects;
using UserManagement.Events;
using Xunit;

namespace UserManagement.Domain.Tests
{
    public sealed class UsersTests
    {
        private readonly IReadOnlyList<User> _usersStorage = new List<User>
        {
            new User(Guid.NewGuid(), "user1", DateTimeOffset.UtcNow),
            new User(Guid.NewGuid(), "user2", DateTimeOffset.UtcNow),
            new User(Guid.NewGuid(), "user3", DateTimeOffset.UtcNow),
        };

        private readonly Users _sut;

        public UsersTests()
        {
            var state = new ConcurrentDictionary<Guid, User>();
            foreach (var user in _usersStorage)
            {
                state.TryAdd(user.Id, user);
            }

            _sut = new Users(state);
        }

        [Fact]
        public void Given_null_state_when_creating_should_initialize_with_empty_state()
        {
            // Given
            // When
            var result = new Users();

            // Then
            result.State.Should().BeEmpty();
        }

        [Theory, AutoData]
        public void Given_empty_state_when_getting_user_by_id_should_return_not_found(Guid id)
        {
            // Given
            var users = new Users(new ConcurrentDictionary<Guid, User>());

            // When
            var result = users.GetUserById(id);

            // Then
            result.Should().BeOfType<UserNotFound>();
            result.As<UserNotFound>().Id.Should().Be(id);
        }

        [Fact]
        public void Given_empty_state_when_getting_all_users_should_return_empty_set()
        {
            // Given
            var users = new Users(new ConcurrentDictionary<Guid, User>());

            // When
            var result = users.GetAllUsers(10, 0);

            // Then
            result.Should().BeOfType<UsersRetrieved>();
            result.As<UsersRetrieved>().Users.Should().BeEmpty();
            result.As<UsersRetrieved>().TotalCount.Should().Be(0);
        }

        [Theory, AutoData]
        public void Given_empty_state_when_deleting_user_should_return_not_found(Guid id)
        {
            // Given
            var users = new Users(new ConcurrentDictionary<Guid, User>());

            // When
            var result = users.DeleteUser(id);

            // Then
            result.Should().BeOfType<UserNotFound>();
            result.As<UserNotFound>().Id.Should().Be(id);
        }

        [Theory, AutoData]
        public void Given_user_not_found_when_getting_user_by_id_should_return_not_found(Guid id)
        {
            // Given
            // When
            var result = _sut.GetUserById(id);

            // Then
            result.Should().BeOfType<UserNotFound>();
            result.As<UserNotFound>().Id.Should().Be(id);
        }

        [Fact]
        public void Given_user_found_when_getting_user_by_id_should_return_user_found()
        {
            // Given
            var existingUser = _usersStorage.First();

            // When
            var result = _sut.GetUserById(existingUser.Id);

            // Then
            result.Should().BeOfType<UserRetrieved>();
            result.As<UserRetrieved>().Id.Should().Be(existingUser.Id);
            result.As<UserRetrieved>().Name.Should().Be(existingUser.Name);
            result.As<UserRetrieved>().CreatedOn.Should().Be(existingUser.CreatedOn);
        }

        [Theory, AutoData]
        public void Given_user_not_found_when_deleting_user_should_return_not_found(Guid id)
        {
            // Given
            // When
            var result = _sut.GetUserById(id);

            // Then
            result.Should().BeOfType<UserNotFound>();
            result.As<UserNotFound>().Id.Should().Be(id);
        }

        [Fact]
        public void Given_user_found_when_deleting_user_should_return_user_deleted()
        {
            // Given
            var existingUser = _usersStorage.First();

            // When
            var result = _sut.DeleteUser(existingUser.Id);

            // Then
            result.Should().BeOfType<UserDeleted>();
            result.As<UserDeleted>().Id.Should().Be(existingUser.Id);
        }

        [Theory, AutoData]
        public void Given_existing_user_id_found_when_creating_user_should_return_user_retrieved(string name)
        {
            // Given
            var existingUser = _usersStorage.First();

            // When
            var result = _sut.CreateUser(existingUser.Id, name);

            // Then
            result.Should().BeOfType<UserRetrieved>();
            result.As<UserRetrieved>().Id.Should().Be(existingUser.Id);
            result.As<UserRetrieved>().Name.Should().Be(existingUser.Name);
            result.As<UserRetrieved>().CreatedOn.Should().Be(existingUser.CreatedOn);
        }

        [Theory, AutoData]
        public void Given_new_user_id_when_creating_user_should_return_user_created(Guid id, string name)
        {
            // Given
            // When
            var result = _sut.CreateUser(id, name);

            // Then
            result.Should().BeOfType<UserCreated>();
            result.As<UserCreated>().Id.Should().Be(id);
            result.As<UserCreated>().Name.Should().Be(name);
            result.As<UserCreated>().CreatedOn.Should().BeCloseTo(DateTimeOffset.UtcNow, 20_000);
        }

        [Theory, AutoData]
        public void Given_created_on_date_provided_when_creating_user_should_use_the_provided_date(Guid id, string name, DateTimeOffset createdOn)
        {
            // Given
            // When
            var result = _sut.CreateUser(id, name, createdOn);

            // Then
            result.Should().BeOfType<UserCreated>();
            result.As<UserCreated>().Id.Should().Be(id);
            result.As<UserCreated>().Name.Should().Be(name);
            result.As<UserCreated>().CreatedOn.Should().Be(createdOn);
        }

        [Theory, AutoData]
        public void Given_empty_state_when_getting_all_users_should_return_empty_list(int limit, int skip)
        {
            // Given
            var users = new Users(new ConcurrentDictionary<Guid, User>());

            // When
            var result = users.GetAllUsers(limit, skip);

            // Then
            result.Should().BeOfType<UsersRetrieved>();
            result.As<UsersRetrieved>().TotalCount.Should().Be(0);
            result.As<UsersRetrieved>().Users.Should().BeEmpty();
        }

        [Theory, AutoData]
        public void Given_bigger_limit_filter_than_there_are_users_in_state_when_getting_all_users_should_return_request_number_of_users([Range(100, 1000)]int limit)
        {
            // Given
            // When
            var result = _sut.GetAllUsers(limit, 0);

            // Then
            result.Should().BeOfType<UsersRetrieved>();
            result.As<UsersRetrieved>().TotalCount.Should().Be(_usersStorage.Count);
            result.As<UsersRetrieved>().Users.Should().HaveCount(_usersStorage.Count);
            result.As<UsersRetrieved>().Users.Select(u => u.Id)
                .Should().BeEquivalentTo(_usersStorage.Select(u => u.Id));
        }

        [Theory, AutoData]
        public void Given_single_user_in_state_when_getting_all_users_should_return_the_user(User user, int limit)
        {
            // Given
            var state = new ConcurrentDictionary<Guid, User>();
            state.TryAdd(user.Id, user);
            var users = new Users(state);

            // When
            var result = users.GetAllUsers(limit, 0);

            // Then
            result.Should().BeOfType<UsersRetrieved>();
            result.As<UsersRetrieved>().TotalCount.Should().Be(1);
            result.As<UsersRetrieved>().Users.Should().HaveCount(1);
            var userRetrieved = result.As<UsersRetrieved>().Users.Single();
            userRetrieved.Id.Should().Be(user.Id);
            userRetrieved.Name.Should().Be(user.Name);
            userRetrieved.CreatedOn.Should().Be(user.CreatedOn);
        }

        [Fact]
        public void Given_10_users_when_limit_100_but_skipping_5_when_getting_all_users_should_return_5_users()
        {
            // Given
            var fixture = new Fixture();
            var state = new ConcurrentDictionary<Guid, User>();
            for (var i = 0; i < 10; i++)
            {
                var user = fixture.Create<User>();
                state.TryAdd(user.Id, user);
            }

            var users = new Users(state);

            // When
            var result = users.GetAllUsers(100, 5);

            // Then
            result.Should().BeOfType<UsersRetrieved>();
            result.As<UsersRetrieved>().TotalCount.Should().Be(10);
            result.As<UsersRetrieved>().Users.Should().HaveCount(5);
        }

        [Fact]
        public void Given_10_users_when_limit_5_and_skipping_7_when_getting_all_users_should_return_5_users_offset()
        {
            // Given
            var fixture = new Fixture();
            var state = new ConcurrentDictionary<Guid, User>();
            for (var i = 0; i < 10; i++)
            {
                var user = fixture.Create<User>();
                state.TryAdd(user.Id, user);
            }

            var users = new Users(state);

            // When
            var result = users.GetAllUsers(5, 7);

            // Then
            result.Should().BeOfType<UsersRetrieved>();
            result.As<UsersRetrieved>().TotalCount.Should().Be(10);
            result.As<UsersRetrieved>().Users.Should().HaveCount(3);
        }
    }
}
